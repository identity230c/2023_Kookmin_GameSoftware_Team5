using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

namespace GSC
{
	[AddComponentMenu("GSC Textbox", 1)]
	public class GSCTextbox : TextMeshProUGUI
	{
		[SerializeField] float m_typeInterval = 0.01f;

		// Real text content
		readonly StringBuilder m_contentBuilder = new();
		// Use for build textbox content
		readonly StringBuilder m_textboxBuilder = new();

		public bool ForceCompleteType = false;

		public override string text =>
			m_contentBuilder.ToString();

		IEnumerator m_typingCoroutine = null;

		protected override void OnDestroy()
		{
			StopPrevEffect();
			base.OnDestroy();
		}

		public void StartTyping()
		{
			StopPrevEffect();
			m_typingCoroutine = Typing();
			StartCoroutine(m_typingCoroutine);
		}

		void StopPrevEffect()
		{
			if (m_typingCoroutine is not null)
			{
				StopCoroutine(m_typingCoroutine);
				ForceCompleteType = false;
			}
		}

		IEnumerator Typing()
		{
			var effect = EffectGenerator();

			while (effect.MoveNext())
			{
				if (ForceCompleteType)
				{
					base.SetText(m_contentBuilder);
					break;
				}

				yield return new WaitForSeconds(m_typeInterval);
			}
		}

		IEnumerator EffectGenerator()
		{
			m_textboxBuilder.Clear();
			yield return null;

			for (int i = 0; i < m_contentBuilder.Length; i++)
			{
				m_textboxBuilder.Append(m_contentBuilder[i]);
				base.SetText(m_textboxBuilder);

				// Do not move this code above!
				// It can cause out of index exception
				yield return null;
			}
		}

		public new void SetText(StringBuilder sb)
		{
			Clear();
			AddText(sb);
		}

		public void SetText(string str)
		{
			Clear();
			AddText(str);
		}

		public void SetText(char ch)
		{
			Clear();
			AddText(ch);
		}

		public void AddText(StringBuilder sb) =>
			m_contentBuilder.Append(sb);

		public void AddText(string str) =>
			m_contentBuilder.Append(str);

		public void AddText(char ch) =>
			m_contentBuilder.Append(ch);

		public void Clear()
		{
			base.SetText(string.Empty);
			m_contentBuilder.Clear();
		}
	}
}
