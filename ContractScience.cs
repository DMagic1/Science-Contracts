#region license
/*The MIT License (MIT)
Science Contract Generator Module

Copyright (c) 2014 DMagic

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Contracts;
using Contracts.Parameters;
using KSPAchievements;

namespace Contract_Science
{

	#region Contract Generator

	public class ContractScience : Contract
	{
		private CelestialBody body = null;
		private ScienceExperiment exp = null;
		private ScienceSubject sub = null;
		private ExperimentSituations targetSituation;
		private contractScienceContainer scienceContainer;
		private AvailablePart aPart = null;
		private ProtoTechNode pTechNode = null;
		private List<ExperimentSituations> situations;
		private string biome = "";
		private string name;
		private System.Random rand = ContractScienceUtils.rand;

		#region overrides

		protected override bool Generate()
		{
			scienceContainer = ContractScienceUtils.availableScience.ElementAt(rand.Next(0, ContractScienceUtils.availableScience.Count)).Value;
			name = ContractScienceUtils.availableScience.FirstOrDefault(n => n.Value == scienceContainer).Key;
			ContractScienceUtils.DebugLog("Generating Contract Now");
			if (scienceContainer.sciPart != "None")
			{
				ContractScienceUtils.DebugLog("Standard Experiment Generating");
				pTechNode = ResearchAndDevelopment.Instance.GetTechState(scienceContainer.sciNode);
				if (pTechNode == null)
					return false;
				else
				{
					ContractScienceUtils.DebugLog("Tech Node Found");
					if (pTechNode.state != RDTech.State.Available)
						return false;
					else
					{
						ContractScienceUtils.DebugLog("Tech Node Researched");
						aPart = pTechNode.partsPurchased.FirstOrDefault(p => p.name == scienceContainer.sciPart);
						if (aPart == null)
							return false;
						ContractScienceUtils.DebugLog("Part: [{0}] Purchased", aPart.name);
					}
				}
			}

			if (body == null)
			{
				body = nextTargetBody();
				if (body == null)
					return false;
			}
			if (exp == null)
			{
				exp = scienceContainer.exp;
				if (exp == null)
					return false;
			}

			situations = availableSituations((int)exp.situationMask, body);

			if (situations.Count == 0)
				return false;
			else
			{
				ContractScienceUtils.DebugLog("Acceptable Situations Found");
				targetSituation = situations[rand.Next(0, situations.Count)];
				ContractScienceUtils.DebugLog("Experimental Situation: {0}", targetSituation);
			}

			if (exp.BiomeIsRelevantWhile(targetSituation))
			{
				ContractScienceUtils.DebugLog("Checking For Biome Usage");
				int i = rand.Next(0, 2);
				if (i == 0)
					biome = fetchBiome(body);
			}

			sub = ResearchAndDevelopment.GetExperimentSubject(exp, targetSituation, body, biome);

			if (sub == null)
			{
				ContractScienceUtils.DebugLog("No Acceptable Science Subject Found");
				return false;
			}
			else
			{
				ContractScienceUtils.DebugLog("Acceptable Science Subject Found");
				if (sub.scientificValue < 0.4f)
					return false;
			}

			if (scienceContainer.agent != "Any")
				this.agent = Contracts.Agents.AgentList.Instance.GetAgent(scienceContainer.agent);

			this.AddParameter(new FurtherCollectScience(body, targetSituation, sub, exp, biome), null);
			ContractScienceUtils.DebugLog("Parameter Added");
			base.SetExpiry(10 * sub.subjectValue, Math.Max(15, 15 * sub.subjectValue) * (float)(this.prestige + 1));
			base.SetScience(Math.Max(exp.baseValue, (exp.baseValue * sub.subjectValue) / 2) * ContractScienceUtils.science, body);
			base.SetDeadlineDays(20f * sub.subjectValue * (float)(this.prestige + 1), body);
			base.SetReputation(5f * (float)(this.prestige + 1), 10f * (float)(this.prestige + 1), body);
			base.SetFunds(100f * sub.subjectValue * ContractScienceUtils.forward, 1000f * sub.subjectValue * ContractScienceUtils.reward, 500f * sub.subjectValue * ContractScienceUtils.penalty, body);
			return true;
		}

		public override bool CanBeCancelled()
		{
			return true;
		}

		public override bool CanBeDeclined()
		{
			return true;
		}

		protected override string GetHashString()
		{
			return sub.id;
		}

		protected override string GetTitle()
		{
			if (targetSituation == ExperimentSituations.InSpaceLow || targetSituation == ExperimentSituations.InSpaceHigh)
				return string.Format("Collect {0} data from orbit around {1}", exp.experimentTitle, body.theName);
			else if (targetSituation == ExperimentSituations.SrfLanded)
				return string.Format("Collect {0} data from the surface of {1}", exp.experimentTitle, body.theName);
			else if (targetSituation == ExperimentSituations.SrfSplashed)
				return string.Format("Collect {0} data from the oceans of {1}", exp.experimentTitle, body.theName);
			else if (targetSituation == ExperimentSituations.FlyingLow || targetSituation == ExperimentSituations.FlyingHigh)
				return string.Format("Collect {0} data during atmospheric flight over {1}", exp.experimentTitle, body.theName);
			return "Stupid Code Is Stupid";
		}

		protected override string GetDescription()
		{
			string story = ContractScienceUtils.storyList[rand.Next(0, ContractScienceUtils.storyList.Count)];
			if (aPart != null)
				return string.Format(story, this.agent.Name, ContractScienceUtils.availableScience.FirstOrDefault(v => v.Value == scienceContainer).Key, body.theName, aPart.title, targetSituation);
			else
				return string.Format(story, this.agent.Name, ContractScienceUtils.availableScience.FirstOrDefault(v => v.Value == scienceContainer).Key, body.theName, "Kerbal", targetSituation);
		}

		protected override string GetSynopsys()
		{
			ContractScienceUtils.DebugLog("Generating Synopsis From {0} Experimental Situation", targetSituation);
			if (!string.IsNullOrEmpty(biome))
			{
				if (targetSituation == ExperimentSituations.InSpaceHigh)
					return string.Format("We need you to record some {0} observations from high orbit above the {2} around {1}", exp.experimentTitle, body.theName, biome);
				else if (targetSituation == ExperimentSituations.InSpaceLow)
					return string.Format("We need you to record some {0} observations from low orbit above the {2} around {1}", exp.experimentTitle, body.theName, biome);
				else if (targetSituation == ExperimentSituations.SrfLanded)
					return string.Format("We need you to record some {0} observations from the {2} while on the surface of {1}", exp.experimentTitle, body.theName, biome);
				else if (targetSituation == ExperimentSituations.SrfSplashed)
					return string.Format("We need you to record some {0} observations from the {2} while on the oceans of {1}", exp.experimentTitle, body.theName, biome);
				else if (targetSituation == ExperimentSituations.FlyingLow)
					return string.Format("We need you to record some {0} observations during low altitude atmospheric flight over the {2} at {1}", exp.experimentTitle, body.theName, biome);
				else if (targetSituation == ExperimentSituations.FlyingHigh)
					return string.Format("We need you to record some {0} observations during high altitude atmospheric flight over the {2} at {1}", exp.experimentTitle, body.theName, biome);
			}
			else
			{
				if (targetSituation == ExperimentSituations.InSpaceHigh)
					return string.Format("We need you to record some {0} observations from high orbit around {1}", exp.experimentTitle, body.theName);
				else if (targetSituation == ExperimentSituations.InSpaceLow)
					return string.Format("We need you to record some {0} observations from low orbit around {1}", exp.experimentTitle, body.theName);
				else if (targetSituation == ExperimentSituations.SrfLanded)
					return string.Format("We need you to record some {0} observations from the surface of {1}", exp.experimentTitle, body.theName);
				else if (targetSituation == ExperimentSituations.SrfSplashed)
					return string.Format("We need you to record some {0} observations from the oceans of {1}", exp.experimentTitle, body.theName);
				else if (targetSituation == ExperimentSituations.FlyingLow)
					return string.Format("We need you to record some {0} observations during low altitude atmospheric flight at {1}", exp.experimentTitle, body.theName);
				else if (targetSituation == ExperimentSituations.FlyingHigh)
					return string.Format("We need you to record some {0} observations during high altitude atmospheric flight at {1}", exp.experimentTitle, body.theName);
			}
			return "Fix Your Stupid Code Idiot...";
		}

		protected override string MessageCompleted()
		{
			return string.Format("You recovered {0} from {1}, well done.", exp.experimentTitle, body.theName);
		}

		protected override void OnLoad(ConfigNode node)
		{
			ContractScienceUtils.DebugLog("Loading Contract");
			int targetBodyID, targetLocation;
			if (int.TryParse(node.GetValue("ScienceTarget"), out targetBodyID))
				body = FlightGlobals.Bodies[targetBodyID];
			ScienceSubject trySub = ResearchAndDevelopment.GetSubjectByID(node.GetValue("ScienceSubject"));
			if (trySub != null)
				sub = trySub;
			name = node.GetValue("ScienceExperiment");
			if (ContractScienceUtils.availableScience.TryGetValue(name, out scienceContainer))
			{
				try
				{
					aPart = ResearchAndDevelopment.Instance.GetTechState(scienceContainer.sciNode).partsPurchased.FirstOrDefault(p => p.name == scienceContainer.sciPart);
				}
				catch
				{
					ContractScienceUtils.DebugLog("No Valid Part Associated With This Experiment");
					aPart = null;
				}
				ScienceExperiment tryExp = scienceContainer.exp;
				if (tryExp != null)
					exp = tryExp;
				ContractScienceUtils.DebugLog("Science Experiment Loaded");
			}
			if (int.TryParse(node.GetValue("ScienceLocation"), out targetLocation))
				targetSituation = (ExperimentSituations)targetLocation;
			biome = node.GetValue("Biome");
		}

		protected override void OnSave(ConfigNode node)
		{
			ContractScienceUtils.DebugLog("Saving Contract");
			node.AddValue("ScienceTarget", body.flightGlobalsIndex);
			node.AddValue("ScienceSubject", sub.id);
			node.AddValue("ScienceExperiment", name);
			node.AddValue("ScienceLocation", (int)targetSituation);
			node.AddValue("Biome", biome);
		}

		public override bool MeetRequirements()
		{
			return true;
		}

		#endregion

		#region Utilities

		private CelestialBody nextTargetBody()
		{
			ContractScienceUtils.DebugLog("Searching For Acceptable Body");
			List<CelestialBody> bList;
			if (this.prestige == ContractPrestige.Trivial)
				return FlightGlobals.Bodies[rand.Next(1, 4)];
			else if (this.prestige == ContractPrestige.Significant)
			{
				bList = GetBodies_Reached(false, true);
				if (bList.Count == 0)
					return null;
				return bList[rand.Next(0, bList.Count)];
			}
			else if (this.prestige == ContractPrestige.Exceptional)
			{
				bList = GetBodies_NextUnreached(4, null);
				if (bList.Contains(FlightGlobals.Bodies[1]))
					bList.Remove(FlightGlobals.Bodies[1]);
				if (bList.Contains(FlightGlobals.Bodies[2]))
					bList.Remove(FlightGlobals.Bodies[2]);
				if (bList.Contains(FlightGlobals.Bodies[3]))
					bList.Remove(FlightGlobals.Bodies[3]);
				return bList[rand.Next(0, bList.Count)];
			}
			return null;
		}

		private List<ExperimentSituations> availableSituations(int i, CelestialBody b)
		{
			ContractScienceUtils.DebugLog("Finding Situations");
			List<ExperimentSituations> expSitList = new List<ExperimentSituations>();
			if (((ExperimentSituations)i & ExperimentSituations.FlyingHigh) == ExperimentSituations.FlyingHigh && b.atmosphere)
				expSitList.Add(ExperimentSituations.FlyingHigh);
			if (((ExperimentSituations)i & ExperimentSituations.FlyingLow) == ExperimentSituations.FlyingLow && b.atmosphere)
				expSitList.Add(ExperimentSituations.FlyingLow);
			if (((ExperimentSituations)i & ExperimentSituations.InSpaceHigh) == ExperimentSituations.InSpaceHigh)
			{
				if (!exp.requireAtmosphere)
					expSitList.Add(ExperimentSituations.InSpaceHigh);
				else if (b.atmosphere)
					expSitList.Add(ExperimentSituations.InSpaceHigh);
			}
			if (((ExperimentSituations)i & ExperimentSituations.InSpaceLow) == ExperimentSituations.InSpaceLow)
			{
				if (!exp.requireAtmosphere)
					expSitList.Add(ExperimentSituations.InSpaceLow);
				else if (b.atmosphere)
					expSitList.Add(ExperimentSituations.InSpaceLow);
			}
			if (((ExperimentSituations)i & ExperimentSituations.SrfLanded) == ExperimentSituations.SrfLanded && b.pqsController != null)
			{
				if (!exp.requireAtmosphere)
					expSitList.Add(ExperimentSituations.SrfLanded);
				else if (b.atmosphere)
					expSitList.Add(ExperimentSituations.SrfLanded);
			}
			if (((ExperimentSituations)i & ExperimentSituations.SrfSplashed) == ExperimentSituations.SrfSplashed && b.ocean && b.pqsController != null)
			{
				if (!exp.requireAtmosphere)
					expSitList.Add(ExperimentSituations.SrfSplashed);
				else if (b.atmosphere)
					expSitList.Add(ExperimentSituations.SrfSplashed);
			}
			ContractScienceUtils.DebugLog("Found {0} Valid Experimental Situations", expSitList.Count);
			return expSitList;
		}

		private string fetchBiome(CelestialBody b)
		{
			ContractScienceUtils.DebugLog("Searching For Biomes");
			string s = "";
			if (b.BiomeMap == null || b.BiomeMap.Map == null)
				return s;
			else
				s = b.BiomeMap.Attributes[rand.Next(0, b.BiomeMap.Attributes.Length)].name;
			ContractScienceUtils.DebugLog("Found Biome: {0}", s);
			return s;
		}

		#endregion

	}

	#endregion

	#region Contract Parameter

	public class FurtherCollectScience : CollectScience
	{
		public CelestialBody body;
		public ExperimentSituations scienceLocation;
		public ScienceSubject subject;
		public ScienceExperiment exp;
		public string biomeName;

		public FurtherCollectScience()
		{
		}

		public FurtherCollectScience(CelestialBody target, ExperimentSituations location, ScienceSubject Subject, ScienceExperiment Exp, string BiomeName)
		{
			body = target;
			scienceLocation = location;
			subject = Subject;
			exp = Exp;
			biomeName = BiomeName;
		}

		protected override string GetHashString()
		{
			return subject.id;
		}

		protected override string GetTitle()
		{
			if (!string.IsNullOrEmpty(biomeName))
			{
				if (scienceLocation == ExperimentSituations.InSpaceHigh)
					return string.Format("Collect {0} data from high orbit around {1}'s {2}", exp.experimentTitle, body.theName, biomeName);
				else if (scienceLocation == ExperimentSituations.InSpaceLow)
					return string.Format("Collect {0} data from low orbit around {1}'s {2}", exp.experimentTitle, body.theName, biomeName);
				else if (scienceLocation == ExperimentSituations.SrfLanded)
					return string.Format("Collect {0} data from the surface at {1}'s {2}", exp.experimentTitle, body.theName, biomeName);
				else if (scienceLocation == ExperimentSituations.SrfSplashed)
					return string.Format("Collect {0} data from the oceans at {1}'s {2}", exp.experimentTitle, body.theName, biomeName);
				else if (scienceLocation == ExperimentSituations.FlyingHigh)
					return string.Format("Collect {0} data during high altitude flight over {1}'s {2}", exp.experimentTitle, body.theName, biomeName);
				else if (scienceLocation == ExperimentSituations.FlyingLow)
					return string.Format("Collect {0} data during low altitude flight over {1}'s {2}", exp.experimentTitle, body.theName, biomeName);
			}
			else
			{
				if (scienceLocation == ExperimentSituations.InSpaceHigh)
					return string.Format("Collect {0} data from high orbit around {1}", exp.experimentTitle, body.theName);
				else if (scienceLocation == ExperimentSituations.InSpaceLow)
					return string.Format("Collect {0} data from low orbit around {1}", exp.experimentTitle, body.theName);
				else if (scienceLocation == ExperimentSituations.SrfLanded)
					return string.Format("Collect {0} data from the surface of {1}", exp.experimentTitle, body.theName);
				else if (scienceLocation == ExperimentSituations.SrfSplashed)
					return string.Format("Collect {0} data from the oceans of {1}", exp.experimentTitle, body.theName);
				else if (scienceLocation == ExperimentSituations.FlyingHigh)
					return string.Format("Collect {0} data during high altitude flight at {1}", exp.experimentTitle, body.theName);
				else if (scienceLocation == ExperimentSituations.FlyingLow)
					return string.Format("Collect {0} data during low altitude flight at {1}", exp.experimentTitle, body.theName);
			}
			return "Stupid Code Is Stupid";
		}

		protected override void OnRegister()
		{
			GameEvents.OnScienceRecieved.Add(scienceRecieve);
		}

		protected override void OnUnregister()
		{
			GameEvents.OnScienceRecieved.Remove(scienceRecieve);
		}

		protected override void OnSave(ConfigNode node)
		{
			ContractScienceUtils.DebugLog("Saving Contract Parameter");
			node.AddValue("ScienceTarget", body.flightGlobalsIndex);
			node.AddValue("ScienceSubject", subject.id);
			node.AddValue("ScienceExperiment", exp.id);
			node.AddValue("ScienceLocation", (int)scienceLocation);
			node.AddValue("Biome", biomeName);
		}

		protected override void OnLoad(ConfigNode node)
		{
			ContractScienceUtils.DebugLog("Loading Contract Parameter");
			int targetBodyID, targetLocation;
			if (int.TryParse(node.GetValue("ScienceTarget"), out targetBodyID))
				body = FlightGlobals.Bodies[targetBodyID];
			ScienceSubject trySub = ResearchAndDevelopment.GetSubjectByID(node.GetValue("ScienceSubject"));
			if (trySub != null)
				subject = trySub;
			ScienceExperiment tryExp = ResearchAndDevelopment.GetExperiment(node.GetValue("ScienceExperiment"));
			if (tryExp != null)
				exp = tryExp;
			if (int.TryParse(node.GetValue("ScienceLocation"), out targetLocation))
				scienceLocation = (ExperimentSituations)targetLocation;
			biomeName = node.GetValue("Biome");
		}

		private void scienceRecieve(float sci, ScienceSubject sub)
		{
			ContractScienceUtils.DebugLog("New Science Results Collected With ID: {0}", sub.id);
			ContractScienceUtils.DebugLog("Comparing To Target Science With ID: {0}", subject.id);
			if (!string.IsNullOrEmpty(biomeName))
			{
				if (sub.id == subject.id)
				{
					ContractScienceUtils.DebugLog("Contract Complete");
					base.SetComplete();
				}
			}
			else
			{
				ContractScienceUtils.DebugLog("Figure Something Out Dummy!!!");
				string clippedSub = sub.id.Replace("@", "");
				string clippedTargetSub = subject.id.Replace("@", "");
				ContractScienceUtils.DebugLog("Comparing New Strings [{0}] And [{1}]", clippedSub, clippedTargetSub);
				if (clippedSub.StartsWith(clippedTargetSub))
				{
					ContractScienceUtils.DebugLog("Contract Complete");
					base.SetComplete();
				}
			}
		}

	}

	#endregion

}