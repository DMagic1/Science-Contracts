#region license
/*The MIT License (MIT)
Science Contract Utilities

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
using System.Linq;
using System.Collections.Generic;
using Contracts;
using UnityEngine;

namespace Contract_Science
{
	#region Utilities

	static class ContractScienceUtils
	{
		internal static System.Random rand;
		internal static Dictionary<string, contractScienceContainer> availableScience;
		internal static float science, reward, forward, penalty;
		internal static List<string> storyList;

		internal static float getSubjectValue(ExperimentSituations s, CelestialBody body)
		{
			float subV = 1;
			if (s == ExperimentSituations.SrfLanded) subV = body.scienceValues.LandedDataValue;
			else if (s == ExperimentSituations.SrfSplashed) subV = body.scienceValues.SplashedDataValue;
			else if (s == ExperimentSituations.FlyingLow || s == ExperimentSituations.FlyingHigh) subV = body.scienceValues.FlyingLowDataValue;
			else if (s == ExperimentSituations.InSpaceLow || s == ExperimentSituations.InSpaceHigh) subV = body.scienceValues.InSpaceLowDataValue;
			return subV;
		}

		internal static void Logging(string s, params object[] stringObjects)
		{
			s = string.Format(s, stringObjects);
			string finalLog = string.Format("[CS] {0}", s);
			Debug.Log(finalLog);
		}

		#region Debug Logging

		[System.Diagnostics.Conditional("DEBUG")]
		internal static void DebugLog(string s, params object[] stringObjects)
		{
			s = string.Format(s, stringObjects);
			string finalLog = string.Format("[CS] {0}", s);
			Debug.Log(finalLog);
		}

		#endregion

	}

	#endregion

	#region Parameter Generators

	static class ContractGenerator
	{
		private static System.Random rand = ContractScienceUtils.rand;

		//Generate fully random science experiment contract parameter
		internal static FurtherCollectScience fetchScienceContract(Contract.ContractPrestige p, List<CelestialBody> cR, List<CelestialBody> cUR)
		{
			contractScienceContainer scienceContainer;
			CelestialBody body;
			ExperimentSituations targetSituation;
			List<ExperimentSituations> situations;
			ScienceExperiment exp;
			ScienceSubject sub;
			AvailablePart aPart;
			string name;
			string biome = "";

			//Choose a random science experiment from our list generated at startup
			scienceContainer = ContractScienceUtils.availableScience.ElementAt(rand.Next(0, ContractScienceUtils.availableScience.Count)).Value;
			name = ContractScienceUtils.availableScience.FirstOrDefault(n => n.Value == scienceContainer).Key;
			ContractScienceUtils.DebugLog("Checking Contract Requirements");

			//Determine if the science part is available if applicable
			if (scienceContainer.sciPart != "None")
			{
				ContractScienceUtils.DebugLog("Checking For Part {0} Now", scienceContainer.sciPart);
				aPart = PartLoader.getPartInfoByName(scienceContainer.sciPart);
				if (aPart == null)
					return null;
				if (!ResearchAndDevelopment.PartModelPurchased(aPart))
					return null;
				ContractScienceUtils.DebugLog("Part: [{0}] Purchased; Contract Meets Requirements", aPart.name);
			}

			//Select a random Celestial Body based on contract prestige levels
			body = nextTargetBody(p, cR, cUR);
			if (body == null)
				return null;

			//Make sure our experiment is OK
			exp = scienceContainer.exp;
			if (exp == null)
				return null;

			//Choose an acceptable experimental situation for a given science experiment
			if ((situations = availableSituations(exp, body)).Count == 0)
				return null;
			else
			{
				ContractScienceUtils.DebugLog("Acceptable Situations Found");
				targetSituation = situations[rand.Next(0, situations.Count)];
				ContractScienceUtils.DebugLog("Experimental Situation: {0}", targetSituation);
			}

			//Build a list of acceptable biomes if applicable, choose one with remaining science
			if (exp.BiomeIsRelevantWhile(targetSituation))
			{
				ContractScienceUtils.DebugLog("Checking For Biome Usage");
				List<string> bList = fetchBiome(body, exp, targetSituation);
				if (bList.Count == 0)
				{
					ContractScienceUtils.DebugLog("Planet All Tapped Out; No Remaining Science Here");
					return null;
				}
				int i = rand.Next(0, 2);
				if (i == 0)
				{
					biome = bList[rand.Next(0, bList.Count)];
					ContractScienceUtils.DebugLog("Acceptable Biome Found: {0}", biome);
				}
			}

			//Make sure that our chosen science subject has science remaining to be gathered
			if ((sub = ResearchAndDevelopment.GetSubjectByID(string.Format("{0}@{1}{2}{3}", exp.id, body.name, targetSituation, biome.Replace(" ", "")))) != null)
			{
				if (sub.scientificValue < 0.4f)
					return null;
			}

			return new FurtherCollectScience(body, targetSituation, biome, name);
		}

		//Generate random experiment for a given celestial body
		internal static FurtherCollectScience fetchScienceContract(CelestialBody body)
		{
			contractScienceContainer scienceContainer;
			ExperimentSituations targetSituation;
			List<ExperimentSituations> situations;
			ScienceExperiment exp;
			ScienceSubject sub;
			AvailablePart aPart;
			string name;
			string biome = "";

			//Choose a random science experiment from our list generated at startup
			scienceContainer = ContractScienceUtils.availableScience.ElementAt(rand.Next(0, ContractScienceUtils.availableScience.Count)).Value;
			name = ContractScienceUtils.availableScience.FirstOrDefault(n => n.Value == scienceContainer).Key;
			ContractScienceUtils.DebugLog("Checking Contract Requirements");

			//Determine if the science part is available if applicable
			if (scienceContainer.sciPart != "None")
			{
				ContractScienceUtils.DebugLog("Checking For Part {0} Now", scienceContainer.sciPart);
				aPart = PartLoader.getPartInfoByName(scienceContainer.sciPart);
				if (aPart == null)
					return null;
				if (!ResearchAndDevelopment.PartModelPurchased(aPart))
					return null;
				ContractScienceUtils.DebugLog("Part: [{0}] Purchased; Contract Meets Requirements", aPart.name);
			}

			//Make sure our experiment is OK
			exp = scienceContainer.exp;
			if (exp == null)
				return null;

			//Choose an acceptable experimental situation for a given science experiment
			if ((situations = availableSituations(exp, body)).Count == 0)
				return null;
			else
			{
				ContractScienceUtils.DebugLog("Acceptable Situations Found");
				targetSituation = situations[rand.Next(0, situations.Count)];
				ContractScienceUtils.DebugLog("Experimental Situation: {0}", targetSituation);
			}

			//Build a list of acceptable biomes if applicable, choose one with remaining science
			if (exp.BiomeIsRelevantWhile(targetSituation))
			{
				ContractScienceUtils.DebugLog("Checking For Biome Usage");
				List<string> bList = fetchBiome(body, exp, targetSituation);
				if (bList.Count == 0)
				{
					ContractScienceUtils.DebugLog("Planet All Tapped Out; No Remaining Science Here");
					return null;
				}
				int i = rand.Next(0, 2);
				if (i == 0)
				{
					biome = bList[rand.Next(0, bList.Count)];
					ContractScienceUtils.DebugLog("Acceptable Biome Found: {0}", biome);
				}
			}

			//Make sure that our chosen science subject has science remaining to be gathered
			if ((sub = ResearchAndDevelopment.GetSubjectByID(string.Format("{0}@{1}{2}{3}", exp.id, body.name, targetSituation, biome.Replace(" ", "")))) != null)
			{
				if (sub.scientificValue < 0.4f)
					return null;
			}

			return new FurtherCollectScience(body, targetSituation, biome, name);
		}

		//Generate experiment for a given Celestial Body and experimental situation
		internal static FurtherCollectScience fetchScienceContract(CelestialBody body, ExperimentSituations targetSituation)
		{
			contractScienceContainer scienceContainer;
			ScienceExperiment exp;
			ScienceSubject sub;
			AvailablePart aPart;
			string name;
			string biome = "";

			//Choose a random science experiment from our list generated at startup
			scienceContainer = ContractScienceUtils.availableScience.ElementAt(rand.Next(0, ContractScienceUtils.availableScience.Count)).Value;
			name = ContractScienceUtils.availableScience.FirstOrDefault(n => n.Value == scienceContainer).Key;
			ContractScienceUtils.DebugLog("Checking Contract Requirements");

			//Determine if the science part is available if applicable
			if (scienceContainer.sciPart != "None")
			{
				ContractScienceUtils.DebugLog("Checking For Part {0} Now", scienceContainer.sciPart);
				aPart = PartLoader.getPartInfoByName(scienceContainer.sciPart);
				if (aPart == null)
					return null;
				if (!ResearchAndDevelopment.PartModelPurchased(aPart))
					return null;
				ContractScienceUtils.DebugLog("Part: [{0}] Purchased; Contract Meets Requirements", aPart.name);
			}

			//Make sure our experiment is OK
			exp = scienceContainer.exp;
			if (exp == null)
				return null;

			//Make sure that the experiment can be conducted in this situation
			if (((ExperimentSituations)exp.situationMask & targetSituation) != targetSituation)
				return null;

			//Build a list of acceptable biomes if applicable, choose one with remaining science
			if (exp.BiomeIsRelevantWhile(targetSituation))
			{
				ContractScienceUtils.DebugLog("Checking For Biome Usage");
				List<string> bList = fetchBiome(body, exp, targetSituation);
				if (bList.Count == 0)
				{
					ContractScienceUtils.DebugLog("Planet All Tapped Out; No Remaining Science Here");
					return null;
				}
				int i = rand.Next(0, 2);
				if (i == 0)
				{
					biome = bList[rand.Next(0, bList.Count)];
					ContractScienceUtils.DebugLog("Acceptable Biome Found: {0}", biome);
				}
			}

			//Make sure that our chosen science subject has science remaining to be gathered
			if ((sub = ResearchAndDevelopment.GetSubjectByID(string.Format("{0}@{1}{2}{3}", exp.id, body.name, targetSituation, biome.Replace(" ", "")))) != null)
			{
				if (sub.scientificValue < 0.4f)
					return null;
			}

			return new FurtherCollectScience(body, targetSituation, biome, name);
		}

		//Generate random experiment for a given celestial body
		internal static FurtherCollectScience fetchScienceContract(CelestialBody body, ScienceExperiment exp)
		{
			contractScienceContainer scienceContainer;
			ExperimentSituations targetSituation;
			List<ExperimentSituations> situations;
			ScienceSubject sub;
			AvailablePart aPart;
			string name;
			string biome = "";

			//Choose science container based on a given science experiment
			scienceContainer = ContractScienceUtils.availableScience.FirstOrDefault(e => e.Value.exp == exp).Value;
			name = ContractScienceUtils.availableScience.FirstOrDefault(n => n.Value == scienceContainer).Key;
			ContractScienceUtils.DebugLog("Checking Contract Requirements");

			//Determine if the science part is available if applicable
			if (scienceContainer.sciPart != "None")
			{
				ContractScienceUtils.DebugLog("Checking For Part {0} Now", scienceContainer.sciPart);
				aPart = PartLoader.getPartInfoByName(scienceContainer.sciPart);
				if (aPart == null)
					return null;
				if (!ResearchAndDevelopment.PartModelPurchased(aPart))
					return null;
				ContractScienceUtils.DebugLog("Part: [{0}] Purchased; Contract Meets Requirements", aPart.name);
			}

			//Make sure our experiment is OK
			exp = scienceContainer.exp;
			if (exp == null)
				return null;

			//Choose an acceptable experimental situation for a given science experiment
			if ((situations = availableSituations(exp, body)).Count == 0)
				return null;
			else
			{
				ContractScienceUtils.DebugLog("Acceptable Situations Found");
				targetSituation = situations[rand.Next(0, situations.Count)];
				ContractScienceUtils.DebugLog("Experimental Situation: {0}", targetSituation);
			}

			//Build a list of acceptable biomes if applicable, choose one with remaining science
			if (exp.BiomeIsRelevantWhile(targetSituation))
			{
				ContractScienceUtils.DebugLog("Checking For Biome Usage");
				List<string> bList = fetchBiome(body, exp, targetSituation);
				if (bList.Count == 0)
				{
					ContractScienceUtils.DebugLog("Planet All Tapped Out; No Remaining Science Here");
					return null;
				}
				int i = rand.Next(0, 2);
				if (i == 0)
				{
					biome = bList[rand.Next(0, bList.Count)];
					ContractScienceUtils.DebugLog("Acceptable Biome Found: {0}", biome);
				}
			}

			//Make sure that our chosen science subject has science remaining to be gathered
			if ((sub = ResearchAndDevelopment.GetSubjectByID(string.Format("{0}@{1}{2}{3}", exp.id, body.name, targetSituation, biome.Replace(" ", "")))) != null)
			{
				if (sub.scientificValue < 0.4f)
					return null;
			}

			return new FurtherCollectScience(body, targetSituation, biome, name);
		}

	#endregion

		#region Contract Properties

		internal static CelestialBody nextTargetBody(Contract.ContractPrestige c, List<CelestialBody> cR, List<CelestialBody> cUR)
		{
			ContractScienceUtils.DebugLog("Searching For Acceptable Body");
			if (c == Contract.ContractPrestige.Trivial)
				return FlightGlobals.Bodies[rand.Next(1, 4)];
			else if (c == Contract.ContractPrestige.Significant)
			{
				if (!cR.Contains(FlightGlobals.Bodies[2]))
					cR.Add(FlightGlobals.Bodies[2]);
				if (!cR.Contains(FlightGlobals.Bodies[3]))
					cR.Add(FlightGlobals.Bodies[3]);
				if (cR.Count == 0)
					return null;
				return cR[rand.Next(0, cR.Count)];
			}
			else if (c == Contract.ContractPrestige.Exceptional)
			{
				if (cUR.Contains(FlightGlobals.Bodies[1]))
					cUR.Remove(FlightGlobals.Bodies[1]);
				if (cUR.Contains(FlightGlobals.Bodies[2]))
					cUR.Remove(FlightGlobals.Bodies[2]);
				if (cUR.Contains(FlightGlobals.Bodies[3]))
					cUR.Remove(FlightGlobals.Bodies[3]);
				if (cUR.Count == 0)
					cUR = cR;
				return cUR[rand.Next(0, cUR.Count)];
			}
			return null;
		}

		internal static List<ExperimentSituations> availableSituations(ScienceExperiment exp, CelestialBody b)
		{
			ContractScienceUtils.DebugLog("Finding Situations");
			List<ExperimentSituations> expSitList = new List<ExperimentSituations>();
			if (((ExperimentSituations)exp.situationMask & ExperimentSituations.FlyingHigh) == ExperimentSituations.FlyingHigh && b.atmosphere)
				expSitList.Add(ExperimentSituations.FlyingHigh);
			if (((ExperimentSituations)exp.situationMask & ExperimentSituations.FlyingLow) == ExperimentSituations.FlyingLow && b.atmosphere)
				expSitList.Add(ExperimentSituations.FlyingLow);
			if (((ExperimentSituations)exp.situationMask & ExperimentSituations.InSpaceHigh) == ExperimentSituations.InSpaceHigh)
			{
				if (!exp.requireAtmosphere)
					expSitList.Add(ExperimentSituations.InSpaceHigh);
				else if (b.atmosphere)
					expSitList.Add(ExperimentSituations.InSpaceHigh);
			}
			if (((ExperimentSituations)exp.situationMask & ExperimentSituations.InSpaceLow) == ExperimentSituations.InSpaceLow)
			{
				if (!exp.requireAtmosphere)
					expSitList.Add(ExperimentSituations.InSpaceLow);
				else if (b.atmosphere)
					expSitList.Add(ExperimentSituations.InSpaceLow);
			}
			if (((ExperimentSituations)exp.situationMask & ExperimentSituations.SrfLanded) == ExperimentSituations.SrfLanded && b.pqsController != null)
			{
				if (!exp.requireAtmosphere)
					expSitList.Add(ExperimentSituations.SrfLanded);
				else if (b.atmosphere)
					expSitList.Add(ExperimentSituations.SrfLanded);
			}
			if (((ExperimentSituations)exp.situationMask & ExperimentSituations.SrfSplashed) == ExperimentSituations.SrfSplashed && b.ocean && b.pqsController != null)
			{
				if (!exp.requireAtmosphere)
					expSitList.Add(ExperimentSituations.SrfSplashed);
				else if (b.atmosphere)
					expSitList.Add(ExperimentSituations.SrfSplashed);
			}
			ContractScienceUtils.DebugLog("Found {0} Valid Experimental Situations", expSitList.Count);
			return expSitList;
		}

		internal static List<string> fetchBiome(CelestialBody b, ScienceExperiment sExp, ExperimentSituations sit)
		{
			ContractScienceUtils.DebugLog("Searching For Biomes");
			List<string> s = new List<string>();
			if (b.BiomeMap == null || b.BiomeMap.Map == null)
			{
				ContractScienceUtils.DebugLog("No Biomes Present For Target Planet");
				s.Add("");
				return s;
			}
			else
			{
				for (int i = 0; i < b.BiomeMap.Attributes.Length; i++)
				{
					string bName = b.BiomeMap.Attributes[i].name;
					ScienceSubject subB = ResearchAndDevelopment.GetSubjectByID(string.Format("{0}@{1}{2}{3}", sExp.id, b.name, sit, bName.Replace(" ", "")));
					if (subB == null)
					{
						s.Add(bName);
						continue;
					}
					else
					{
						if (subB.scientificValue > 0.4f)
							s.Add(bName);
					}
				}
			}
			ContractScienceUtils.DebugLog("Found Acceptable Biomes");
			return s;
		}

		#endregion


	}
}
