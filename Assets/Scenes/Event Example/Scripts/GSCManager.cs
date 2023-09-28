using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;

namespace GSC
{
	[Serializable]
	public class GSCException : UnityException
	{
		public GSCException(string message, int lineIndex)
			: base($"(GSC){message}: line {lineIndex}")
		{
		}
	}

	[Serializable]
	public class GSCInvalidCommandException : GSCException
	{
		public GSCInvalidCommandException(int lineIndex)
			: base("Invalid command", lineIndex)
		{
		}
	}

	[Serializable]
	public class GSCNodeNotFoundException : GSCException
	{
		public GSCNodeNotFoundException(string nodeName, int lineIndex)
			: base($"Node not found({nodeName})", lineIndex)
		{
		}
	}

	[Serializable]
	public class ScriptCallback
	{
		public string name;
		public UnityEvent onCall;
	}

	public partial class GSCManager : MonoBehaviour
	{
		[SerializeField] GSCScript m_GSCScript;

		[SerializeField] GSCText m_GSCTextbox;
		[SerializeField] GameObject m_buttonPrefab;
		[SerializeField] LayoutGroup m_buttonLayout;

		[SerializeField] string m_startNode;
		[SerializeField] char m_prefix;

		public List<ScriptCallback> callbacks;

		const string OPRule = "([A-Za-z]+)";
		const string NameRule = "([A-Za-z_][0-9A-Za-z_]*)";
		const string TextRule = "(\".*\")";
		const string SpaceMustRule = "([ \t]+)";
		const string SpaceMayRule = "([ \t]*)";
		const string AnyRule = "(.*)";
		readonly string ArgRule = $"{NameRule}|{TextRule}";

		readonly Dictionary<string, int> m_nodeLineDict = new();
		string[] m_scriptLines;
		int m_lineIndex;

		bool m_buttonPressed = false;

		IEnumerator scriptCoroutine = null;

		void OnEnable()
		{
			Initialize();

			string scriptText = m_GSCScript.script.Replace("\r", "");
			m_scriptLines = scriptText.Split("\n");

			// Parse script nodes and check if-waitif
			for (int i = 0; i < m_scriptLines.Length; i++)
			{
				string line = m_scriptLines[i].Trim();

				if (Regex.IsMatch(line, $@"^{SpaceMayRule}{m_prefix}node{SpaceMustRule}{NameRule}{SpaceMayRule}$"))
				{
					// Add node to dictionary
					string nodeName = line.Split()[1];

					if (m_nodeLineDict.ContainsKey(nodeName))
						throw new GSCException("Multiple node name definition", i + 1);

					m_nodeLineDict.Add(nodeName, i);
				}
			}

			scriptCoroutine = StartScript();
			StartCoroutine(scriptCoroutine);
		}

		IEnumerator StartScript()
		{
			if (!m_nodeLineDict.TryGetValue(m_startNode, out m_lineIndex))
				throw new GSCException($"Invalid start node ({m_startNode})", -1);

			string nowNode = m_startNode;

			Debug.Log($"(GSC)Start script: node {nowNode}, line {m_lineIndex + 1}");

			bool ifStateOpened = false;
			bool keepLoop = true;

			while (m_lineIndex < m_scriptLines.Length && keepLoop)
			{
				string line = m_scriptLines[m_lineIndex++];

				// Textbox output
				if (!Regex.IsMatch(line, $"^{SpaceMayRule}{m_prefix}"))
				{
					m_GSCTextbox.AddText(line);
					continue;
				}

				// Remove prefix from cmd
				string cmd = line.Trim()[1..];
				string op, args;
				Match match;

				if (!(match = Regex.Match(cmd, $"^{OPRule}{AnyRule}$")).Success)
					throw new GSCInvalidCommandException(m_lineIndex);

				// Parse cmd to op and args
				op = match.Groups[1].Value;
				args = match.Groups[2].Value;

				// Divide by argument type
				// No arguments
				if (args == string.Empty)
				{
					switch (op)
					{
						case "waitif":
							if (!ifStateOpened)
								throw new GSCException("If statement not opened", m_lineIndex);

							m_GSCTextbox.StartTyping();
							yield return new WaitUntil(() => m_buttonPressed);

							m_buttonPressed = false;
							ifStateOpened = false;

							break;

						default:
							throw new GSCInvalidCommandException(m_lineIndex);
					}
				}
				// One argument
				else if ((match = Regex.Match(args, $"^{SpaceMustRule}{ArgRule}$")).Success)
				{
					string arg = args.TrimStart();

					// Argument is Text
					if (Regex.IsMatch(arg, TextRule))
					{
						string scenePath = arg.Trim('"');

						switch (op)
						{
							case "scene":
								if (SceneUtility.GetBuildIndexByScenePath(scenePath + "") != -1)
								{
									var task = SceneManager.LoadSceneAsync(scenePath);

									yield return new WaitUntil(() => task.isDone);
									keepLoop = false;

									break;
								}
								else
									throw new GSCException($"Scene not exist({scenePath})", m_lineIndex);

							default:
								throw new GSCInvalidCommandException(m_lineIndex);
						}
					}
					// Argument is Name
					else if (Regex.IsMatch(arg, NameRule))
					{
						string nodeName = arg;
						string callbackName = arg;

						switch (op)
						{
							case "node":
								if (nowNode != nodeName)
									keepLoop = false;

								break;

							case "call":
								bool callbackNotFound = true;

								foreach (var callback in callbacks)
								{
									if (callback.name == callbackName)
									{
										callback.onCall.Invoke();
										callbackNotFound = false;
										break;
									}
								}

								if (callbackNotFound)
									throw new GSCException($"Callback not found({callbackName})", m_lineIndex);

								break;

							default:
								throw new GSCInvalidCommandException(m_lineIndex);
						}
					}
					else
						throw new GSCInvalidCommandException(m_lineIndex);
				}
				// Two arguments
				else if ((match = Regex.Match(args, $"^{SpaceMustRule}{ArgRule}{SpaceMustRule}{ArgRule}$")).Success)
				{
					// Index 0 is entire,
					// Index 1 is SpaceRule,
					// Index 2 is NameRule, v
					// Index 3 is TextRule, v
					// Index 4 is SpaceRule,
					// Index 5 is NameRule, v
					// Index 6 is TextRule, v
					// This sucks.
					string arg1 = match.Groups[match.Groups[2].Value != "" ? 2 : 3].Value;
					string arg2 = match.Groups[match.Groups[5].Value != "" ? 5 : 6].Value;

					switch (op)
					{
						case "if":
							if (!Regex.IsMatch(arg1, TextRule) || !Regex.IsMatch(arg2, NameRule))
								throw new GSCInvalidCommandException(m_lineIndex);

							ifStateOpened = true;

							string buttonText = arg1.Trim('"');
							string nodeName = arg2;

							if (!m_nodeLineDict.TryGetValue(nodeName, out int nextNodeIndex))
								throw new GSCNodeNotFoundException(nodeName, m_lineIndex);

							var buttonObject = Instantiate(m_buttonPrefab, m_buttonLayout.transform);
							var button = buttonObject.GetComponent<Button>();

							button.onClick.RemoveAllListeners();
							button.onClick.AddListener(() =>
							{
								nowNode = GotoNode(nextNodeIndex);
								m_buttonPressed = true;
							});

							buttonObject.GetComponentInChildren<TMP_Text>().text = buttonText;
							break;

						default:
							throw new GSCInvalidCommandException(m_lineIndex);
					}
				}
				else
					throw new GSCInvalidCommandException(m_lineIndex);
			}

			if (ifStateOpened)
				throw new GSCException("Unclosed if statement detected", m_lineIndex);

			m_GSCTextbox.StartTyping();
			Debug.Log($"(GSC)End script: node {nowNode}, line {m_lineIndex}");
		}

		void Initialize()
		{
			if (scriptCoroutine != null)
				StopCoroutine(scriptCoroutine);

			m_GSCTextbox.Clear();
			m_buttonPressed = false;
			m_nodeLineDict.Clear();
			ClearButtonLayout();
		}

		// Destroy all buttons from layout
		void ClearButtonLayout()
		{
			foreach (var childButton in m_buttonLayout.GetComponentsInChildren<Button>(true))
			{
				var childObject = childButton.gameObject;
				Destroy(childObject);
			}
		}

		// Goto the node in line nodeLineIndex and return that node name.
		string GotoNode(int nodeLineIndex)
		{
			m_GSCTextbox.Clear();

			ClearButtonLayout();

			m_lineIndex = nodeLineIndex;

			return m_nodeLineDict.First(pair => pair.Value == m_lineIndex).Key;
		}
	}
}
