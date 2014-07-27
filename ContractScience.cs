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
		internal FurtherCollectScience newParam;
		private CelestialBody body;
		private ExperimentSituations targetSituation;
		private contractScienceContainer scienceContainer;
		private AvailablePart aPart = null;
		private string biome = "";
		private string name;
		private string subject;
		private float subjectValue;
		private System.Random rand = ContractScienceUtils.rand;

		#region overrides

		protected override bool Generate()
		{
			if (!GetBodies_Reached(true, true).Contains(FlightGlobals.Bodies[1]))
				return false;
			if (ContractSystem.Instance.GetCurrentContracts<ContractScience>().Count() > 2)
				return false;

			//Generates the science experiment, returns null if experiment fails any check
			if ((newParam = ContractGenerator.fetchScienceContract(this.prestige, GetBodies_Reached(false, true), GetBodies_NextUnreached(4, null))) == null)
				return false;

			//Set various parameters to be used for the title, rewards, descriptions, etc...
			body = newParam.Body;
			targetSituation = newParam.Sit;
			scienceContainer = newParam.Container;
			biome = newParam.Biome;
			subject = newParam.Subject;
			name = newParam.Name;
			subjectValue = ContractScienceUtils.getSubjectValue(targetSituation, body);
			try
			{
				aPart = PartLoader.getPartInfoByName(scienceContainer.sciPart);
				ContractScienceUtils.DebugLog("Part: [{0}] Assigned", aPart.name);
			}
			catch
			{
				ContractScienceUtils.DebugLog("No Valid Part Associated With This Experiment");
				aPart = null;
			}

			//Sets the agent if specified, choose random agent otherwise with higher chance given to part's manufacturer
			if (scienceContainer.agent != "Any")
				this.agent = Contracts.Agents.AgentList.Instance.GetAgent(scienceContainer.agent);
			else
				if (aPart != null)
					if (rand.Next(0, 3) != 0)
					{
						this.agent = Contracts.Agents.AgentList.Instance.GetPartManufacturer(aPart);
						ContractScienceUtils.DebugLog("Assigning Part Manufacturer: [{0}] As Agent", this.agent.Name);
					}

			//Add the parameter and set the rewards
			this.AddParameter(newParam, null);
			ContractScienceUtils.DebugLog("Parameter Added");
			base.SetExpiry(10 * subjectValue, Math.Max(15, 15 * subjectValue) * (float)(this.prestige + 1));
			base.SetScience(Math.Max(scienceContainer.exp.baseValue, (scienceContainer.exp.baseValue * subjectValue) / 3) * ContractScienceUtils.science, body);
			base.SetDeadlineDays(20f * subjectValue * (float)(this.prestige + 1), body);
			base.SetReputation(5f * (float)(this.prestige + 1), 10f * (float)(this.prestige + 1), body);
			base.SetFunds(100f * subjectValue * ContractScienceUtils.forward, 1000f * subjectValue * ContractScienceUtils.reward, 500f * subjectValue * ContractScienceUtils.penalty, body);
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
			return subject;
		}

		protected override string GetTitle()
		{
			if (targetSituation == ExperimentSituations.InSpaceLow || targetSituation == ExperimentSituations.InSpaceHigh)
				return string.Format("Collect {0} data from orbit around {1}", scienceContainer.exp.experimentTitle, body.theName);
			else if (targetSituation == ExperimentSituations.SrfLanded)
				return string.Format("Collect {0} data from the surface of {1}", scienceContainer.exp.experimentTitle, body.theName);
			else if (targetSituation == ExperimentSituations.SrfSplashed)
				return string.Format("Collect {0} data from the oceans of {1}", scienceContainer.exp.experimentTitle, body.theName);
			else if (targetSituation == ExperimentSituations.FlyingLow || targetSituation == ExperimentSituations.FlyingHigh)
				return string.Format("Collect {0} data during atmospheric flight over {1}", scienceContainer.exp.experimentTitle, body.theName);
			return "Stupid Code Is Stupid";
		}

		protected override string GetDescription()
		{
			string story = ContractScienceUtils.storyList[rand.Next(0, ContractScienceUtils.storyList.Count)];
			if (aPart != null)
				return string.Format(story, this.agent.Name, name, body.theName, aPart.title, targetSituation);
			else
				return string.Format(story, this.agent.Name, name, body.theName, "Kerbal", targetSituation);
		}

		protected override string GetSynopsys()
		{
			ContractScienceUtils.DebugLog("Generating Synopsis From {0} Experimental Situation", targetSituation);
			if (!string.IsNullOrEmpty(biome))
			{
				if (targetSituation == ExperimentSituations.InSpaceHigh)
					return string.Format("We need you to record some {0} observations from high orbit above the {2} around {1}", scienceContainer.exp.experimentTitle, body.theName, biome);
				else if (targetSituation == ExperimentSituations.InSpaceLow)
					return string.Format("We need you to record some {0} observations from low orbit above the {2} around {1}", scienceContainer.exp.experimentTitle, body.theName, biome);
				else if (targetSituation == ExperimentSituations.SrfLanded)
					return string.Format("We need you to record some {0} observations from the {2} while on the surface of {1}", scienceContainer.exp.experimentTitle, body.theName, biome);
				else if (targetSituation == ExperimentSituations.SrfSplashed)
					return string.Format("We need you to record some {0} observations from the {2} while on the oceans of {1}", scienceContainer.exp.experimentTitle, body.theName, biome);
				else if (targetSituation == ExperimentSituations.FlyingLow)
					return string.Format("We need you to record some {0} observations during low altitude atmospheric flight over the {2} at {1}", scienceContainer.exp.experimentTitle, body.theName, biome);
				else if (targetSituation == ExperimentSituations.FlyingHigh)
					return string.Format("We need you to record some {0} observations during high altitude atmospheric flight over the {2} at {1}", scienceContainer.exp.experimentTitle, body.theName, biome);
			}
			else
			{
				if (targetSituation == ExperimentSituations.InSpaceHigh)
					return string.Format("We need you to record some {0} observations from high orbit around {1}", scienceContainer.exp.experimentTitle, body.theName);
				else if (targetSituation == ExperimentSituations.InSpaceLow)
					return string.Format("We need you to record some {0} observations from low orbit around {1}", scienceContainer.exp.experimentTitle, body.theName);
				else if (targetSituation == ExperimentSituations.SrfLanded)
					return string.Format("We need you to record some {0} observations from the surface of {1}", scienceContainer.exp.experimentTitle, body.theName);
				else if (targetSituation == ExperimentSituations.SrfSplashed)
					return string.Format("We need you to record some {0} observations from the oceans of {1}", scienceContainer.exp.experimentTitle, body.theName);
				else if (targetSituation == ExperimentSituations.FlyingLow)
					return string.Format("We need you to record some {0} observations during low altitude atmospheric flight at {1}", scienceContainer.exp.experimentTitle, body.theName);
				else if (targetSituation == ExperimentSituations.FlyingHigh)
					return string.Format("We need you to record some {0} observations during high altitude atmospheric flight at {1}", scienceContainer.exp.experimentTitle, body.theName);
			}
			return "Fix Your Stupid Code Idiot...";
		}

		protected override string MessageCompleted()
		{
			return string.Format("You recovered {0} from {1}, well done.", scienceContainer.exp.experimentTitle, body.theName);
		}

		//Parse saved string to set parameters for generating title, descriptions, etc...
		protected override void OnLoad(ConfigNode node)
		{
			ContractScienceUtils.DebugLog("Loading Contract");
			int targetBodyID, targetLocation;
			string[] scienceString = node.GetValue("Science_Subject").Split('|');
			name = scienceString[0];
			if (ContractScienceUtils.availableScience.TryGetValue(name, out scienceContainer))
			{
				try
				{
					aPart = PartLoader.getPartInfoByName(scienceContainer.sciPart);
				}
				catch
				{
					ContractScienceUtils.DebugLog("No Valid Part Associated With This Experiment");
					aPart = null;
				}
			}
			if (int.TryParse(scienceString[1], out targetBodyID))
				body = FlightGlobals.Bodies[targetBodyID];
			if (int.TryParse(scienceString[2], out targetLocation))
				targetSituation = (ExperimentSituations)targetLocation;
			biome = scienceString[3];
			subject = string.Format("{0}@{1}{2}{3}", scienceContainer.exp.id, body.name, targetSituation, biome.Replace(" ", ""));
		}

		//Minimal amount of information saved, a simple string with a | delimiter
		protected override void OnSave(ConfigNode node)
		{
			ContractScienceUtils.DebugLog("Saving Contract");
			node.AddValue("Science_Subject", string.Format("{0}|{1}|{2}|{3}", name, body.flightGlobalsIndex, (int)targetSituation, biome));
		}

		public override bool MeetRequirements()
		{
			return true;
		}

		#endregion

	}

	#endregion
}