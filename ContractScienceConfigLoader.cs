﻿#region license
/*The MIT License (MIT)
Science Contract Config Loader

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

using UnityEngine;

namespace Contract_Science
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	internal class DMConfigLoader : MonoBehaviour
	{

		private void Start()
		{
			ContractScienceUtils.rand = new System.Random();
			ContractScienceUtils.DebugLog("Generating Global Random Number Generator");
			configLoad();
		}

		private void configLoad()
		{
			foreach (ConfigNode setNode in GameDatabase.Instance.GetConfigNodes("CONTRACT_SETTINGS"))
				if (setNode.GetValue("name") == "Contract Settings")
				{
					ContractScienceUtils.science = float.Parse(setNode.GetValue("Global_Science_Return"));
					ContractScienceUtils.reward = float.Parse(setNode.GetValue("Global_Fund_Reward"));
					ContractScienceUtils.forward = float.Parse(setNode.GetValue("Global_Fund_Forward"));
					ContractScienceUtils.penalty = float.Parse(setNode.GetValue("Global_Fund_Penalty"));
					ContractScienceUtils.Logging("Contract Variables Set; Science Reward: {0} ; Completion Reward: {1} ; Forward Amount: {2} ; Penalty Amount: {3}",
						ContractScienceUtils.science, ContractScienceUtils.reward, ContractScienceUtils.forward, ContractScienceUtils.penalty);
					break;
				}
			foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("CONTRACT_EXPERIMENT"))
			{
				string name, part, techNode, agent, expID = "";
				expID = node.GetValue("experimentID");
				ScienceExperiment exp = ResearchAndDevelopment.GetExperiment(expID);
				if (exp != null)
				{
					name = node.GetValue("name");
					part = node.GetValue("part");
					if (node.HasValue("techNode"))
						techNode = node.GetValue("techNode");
					else
						techNode = "None";
					if (node.HasValue("agent"))
						agent = node.GetValue("agent");
					else
						agent = "Any";
					ContractScienceUtils.availableScience.Add(name, new contractScienceContainer(expID, exp, part, techNode, agent));
					ContractScienceUtils.Logging("New Experiment: [{0}] Available For Contracts", exp.experimentTitle);
				}
			}
			//ContractScienceUtils.Logging("Successfully Added {0} New Experiments To Contract List", ContractScienceUtils.availableScience.Count);
			//foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("SCIENCE_STORY_DEF"))
			//{
			//    foreach (ConfigNode storyNode in node.GetNodes("SCIENCE_BACKSTORY"))
			//    {
			//        foreach (string story in storyNode.GetValues("generic"))
			//        {
			//            if (!string.IsNullOrEmpty(story))
			//                ContractScienceUtils.storyList.Add(story);
			//        }
			//    }
			//}
			string[] story = new string[4] {
				@"The scientists at {0} have recently come to the realization that we know very little about {2}. To remedy this they have tasked you with studying this celestial body using a {3} to collect {1} data.",
				@"Recent advances made by {0} scientists have called into question several commonly held beliefs about {2}. By collecting {1} data directly from {2} using a {3} they could further advance our understanding of this celestial body.",
				@"Using a {3}, the scientists at {0} would like you to take a spacecraft to {2} and collect {1} data there. They are willing to reward you handsomely for such a task and to provide some initial Funds for your use.",
				@"A lucrative offer is being made by {0} for the further scientific study of {2}. Using a {3} they want you to collect {1} data and send it back to Kerbin for further study. Generous rewards and initial funding are being offered."};
			foreach (string s in story)
				ContractScienceUtils.storyList.Add(s);
			ContractScienceUtils.Logging("Successfully Added {0} New Backstories to Story List", ContractScienceUtils.storyList.Count);
		}

		private void OnDestroy()
		{
		}

	}

	internal class contractScienceContainer
	{
		internal string name;
		internal ScienceExperiment exp;
		internal string sciPart;
		internal string sciNode;
		internal string agent;

		internal contractScienceContainer(string expName, ScienceExperiment sciExp, string sciPartID, string sciTechNode, string agentName)
		{
			name = expName;
			exp = sciExp;
			sciPart = sciPartID;
			sciNode = sciTechNode;
			agent = agentName;
		}
	}

}
