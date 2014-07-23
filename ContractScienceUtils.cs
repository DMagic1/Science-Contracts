﻿#region license
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Contract_Science
{
	static class ContractScienceUtils
	{
		internal static System.Random rand;
		internal static Dictionary<string, contractScienceContainer> availableScience = new Dictionary<string, contractScienceContainer>();
		internal static float science, reward, forward, penalty;
		internal static List<string> storyList = new List<string>();

		internal static void Logging(string s, params object[] stringObjects)
		{
			s = string.Format(s, stringObjects);
			string finalLog = string.Format("[CS] {0}", s);
			Debug.Log(finalLog);
		}

		#region Debug Logging
#if DEBUG

		internal static void DebugLog(string s, params object[] stringObjects)
		{
			s = string.Format(s, stringObjects);
			string finalLog = string.Format("[CS] {0}", s);
			Debug.Log(finalLog);
		}

#endif
		#endregion


	}
}
