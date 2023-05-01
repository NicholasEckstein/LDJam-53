using UnityEngine;
using System.Collections;

public class FillbarAttribute : PropertyAttribute
{
	public float MaxValue { get; private set; }
	public string SerializedMemberVariableName { get; private set; } = "";

	public FillbarAttribute(float maxValue)
	{
		MaxValue = maxValue;
	}
	public FillbarAttribute(string serializedMemberVariableName)
	{
		SerializedMemberVariableName = serializedMemberVariableName;
	}
}