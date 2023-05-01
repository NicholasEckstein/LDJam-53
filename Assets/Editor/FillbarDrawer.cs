using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


[CustomPropertyDrawer(typeof(FillbarAttribute))]
public class FillbarDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		if (EditorApplication.isPlaying)
		{
			return EditorGUIUtility.singleLineHeight;
		}
		else
		{
			return 0.0f;
		}
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		if (EditorApplication.isPlaying)
		{
			GUI.enabled = false;
			FillbarAttribute fillAttribute = (FillbarAttribute)attribute;
			float maxValue;
			if (!string.IsNullOrEmpty(fillAttribute.SerializedMemberVariableName))
			{
				maxValue = property.serializedObject.FindProperty(fillAttribute.SerializedMemberVariableName).floatValue;
			}
			else
			{
				maxValue = fillAttribute.MaxValue;
			}
			EditorGUI.BeginProperty(position, label, property);
			//property.floatValue = EditorGUI.Slider(position, label, property.floatValue, 0.0f, max);

			if (maxValue != 0.0f)
			{
				float labelWidth = EditorGUIUtility.labelWidth;
				Rect labelRect = new Rect(position.x, position.y, labelWidth, position.height);
				EditorGUI.LabelField(labelRect, label);

				Rect fieldRect = new Rect(position.x + labelWidth, position.y, position.width - labelWidth, EditorGUIUtility.singleLineHeight);
				//property.floatValue = EditorGUI.FloatField(fieldRect, GUIContent.none, property.floatValue);

				Rect fillBarRect = new Rect(fieldRect.x, position.y, fieldRect.width, EditorGUIUtility.singleLineHeight);
				EditorGUI.DrawRect(fillBarRect, Color.gray);

				float fillPercentage = Mathf.Clamp01(property.floatValue / maxValue);
				Rect fillRect = new Rect(fillBarRect.x, fillBarRect.y, fillBarRect.width * fillPercentage, fillBarRect.height);
				EditorGUI.DrawRect(fillRect, Color.green);
			}

			EditorGUI.EndProperty();
			GUI.enabled = true;
		}
	}
}