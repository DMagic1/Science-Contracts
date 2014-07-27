#region license
/*The MIT License (MIT)
Science Contract Parameter

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
using Contracts.Parameters;

namespace Contract_Science
{
	public class FurtherCollectScience: CollectScience
	{
		private CelestialBody body;
		private ExperimentSituations scienceLocation;
		private string subject;
		private string biomeName;
		private string name;
		private contractScienceContainer scienceContainer;

		public FurtherCollectScience()
		{
			ContractScienceUtils.DebugLog("Run Empty Constructor");
		}

		internal FurtherCollectScience(CelestialBody target, ExperimentSituations location, string BiomeName, string Name)
		{
			ContractScienceUtils.DebugLog("Run Initial Constructor");
			body = target;
			scienceLocation = location;
			biomeName = BiomeName;
			name = Name;
			ContractScienceUtils.availableScience.TryGetValue(Name, out scienceContainer);
			subject = string.Format("{0}@{1}{2}{3}", scienceContainer.exp.id, body.name, scienceLocation, biomeName.Replace(" ", ""));
		}

		//Properties to be accessed by parent contract
		internal CelestialBody Body
		{
			get { return body; }
			private set { }
		}

		internal ExperimentSituations Sit
		{
			get { return scienceLocation; }
			private set { }
		}

		internal string Biome
		{
			get { return biomeName; }
			private set { }
		}

		internal string Subject
		{
			get { return subject; }
			private set { }
		}

		internal contractScienceContainer Container
		{
			get { return scienceContainer; }
			private set { }
		}

		internal string Name
		{
			get { return name; }
			private set { }
		}

		protected override string GetHashString()
		{
			return subject;
		}

		protected override string GetTitle()
		{
			if (!string.IsNullOrEmpty(biomeName))
			{
				if (scienceLocation == ExperimentSituations.InSpaceHigh)
					return string.Format("Collect {0} data from high orbit around {1}'s {2}", scienceContainer.exp.experimentTitle, body.theName, biomeName);
				else if (scienceLocation == ExperimentSituations.InSpaceLow)
					return string.Format("Collect {0} data from low orbit around {1}'s {2}", scienceContainer.exp.experimentTitle, body.theName, biomeName);
				else if (scienceLocation == ExperimentSituations.SrfLanded)
					return string.Format("Collect {0} data from the surface at {1}'s {2}", scienceContainer.exp.experimentTitle, body.theName, biomeName);
				else if (scienceLocation == ExperimentSituations.SrfSplashed)
					return string.Format("Collect {0} data from the oceans at {1}'s {2}", scienceContainer.exp.experimentTitle, body.theName, biomeName);
				else if (scienceLocation == ExperimentSituations.FlyingHigh)
					return string.Format("Collect {0} data during high altitude flight over {1}'s {2}", scienceContainer.exp.experimentTitle, body.theName, biomeName);
				else if (scienceLocation == ExperimentSituations.FlyingLow)
					return string.Format("Collect {0} data during low altitude flight over {1}'s {2}", scienceContainer.exp.experimentTitle, body.theName, biomeName);
			}
			else
			{
				if (scienceLocation == ExperimentSituations.InSpaceHigh)
					return string.Format("Collect {0} data from high orbit around {1}", scienceContainer.exp.experimentTitle, body.theName);
				else if (scienceLocation == ExperimentSituations.InSpaceLow)
					return string.Format("Collect {0} data from low orbit around {1}", scienceContainer.exp.experimentTitle, body.theName);
				else if (scienceLocation == ExperimentSituations.SrfLanded)
					return string.Format("Collect {0} data from the surface of {1}", scienceContainer.exp.experimentTitle, body.theName);
				else if (scienceLocation == ExperimentSituations.SrfSplashed)
					return string.Format("Collect {0} data from the oceans of {1}", scienceContainer.exp.experimentTitle, body.theName);
				else if (scienceLocation == ExperimentSituations.FlyingHigh)
					return string.Format("Collect {0} data during high altitude flight at {1}", scienceContainer.exp.experimentTitle, body.theName);
				else if (scienceLocation == ExperimentSituations.FlyingLow)
					return string.Format("Collect {0} data during low altitude flight at {1}", scienceContainer.exp.experimentTitle, body.theName);
			}
			return "Stupid Code Is Stupid";
		}

		protected override void OnRegister()
		{
			ContractScienceUtils.DebugLog("Parameter On Register");
			GameEvents.OnScienceRecieved.Add(scienceRecieve);
		}

		protected override void OnUnregister()
		{
			ContractScienceUtils.DebugLog("Parameter On UnRegister");
			GameEvents.OnScienceRecieved.Remove(scienceRecieve);
		}

		//Save and load information for generating title
		protected override void OnSave(ConfigNode node)
		{
			ContractScienceUtils.DebugLog("Saving Contract Parameter");
			node.AddValue("Science_Subject", string.Format("{0}|{1}|{2}|{3}", name, body.flightGlobalsIndex, (int)scienceLocation, biomeName));
		}

		protected override void OnLoad(ConfigNode node)
		{
			ContractScienceUtils.DebugLog("Loading Contract Parameter");
			int targetBodyID, targetLocation;
			string[] scienceString = node.GetValue("Science_Subject").Split('|');
			ContractScienceUtils.availableScience.TryGetValue(scienceString[0], out scienceContainer);
			if (int.TryParse(scienceString[1], out targetBodyID))
				body = FlightGlobals.Bodies[targetBodyID];
			if (int.TryParse(scienceString[2], out targetLocation))
				scienceLocation = (ExperimentSituations)targetLocation;
			biomeName = scienceString[3];
			subject = string.Format("{0}@{1}{2}{3}", scienceContainer.exp.id, body.name, scienceLocation, biomeName.Replace(" ", ""));
		}

		//Check if submitted science is valid to complete contract
		private void scienceRecieve(float sci, ScienceSubject sub)
		{
			ContractScienceUtils.DebugLog("New Science Results Collected With ID: {0}", sub.id);
			ContractScienceUtils.DebugLog("Comparing To Target Science With ID: {0}", subject);
			if (!string.IsNullOrEmpty(biomeName))
			{
				if (sub.id == subject)
				{
					ContractScienceUtils.DebugLog("Contract Complete");
					base.SetComplete();
				}
			}
			else
			{
				//Contracts not specifying a biome will not match submitted subject id if the experiment uses biomes
				//This method clips the @ symbol and matches the beginning of the subject ids
				string clippedSub = sub.id.Replace("@", "");
				string clippedTargetSub = subject.Replace("@", "");
				ContractScienceUtils.DebugLog("Comparing New Strings [{0}] And [{1}]", clippedSub, clippedTargetSub);
				if (clippedSub.StartsWith(clippedTargetSub))
				{
					if (sci < (scienceContainer.exp.baseValue * sub.subjectValue * 0.4f))
						ScreenMessages.PostScreenMessage("This area has already been studied, try investigating another region to complete the contract", 8f, ScreenMessageStyle.UPPER_CENTER);
					else
					{
						ContractScienceUtils.DebugLog("Contract Complete");
						base.SetComplete();
					}
				}
			}
		}
	}
}
