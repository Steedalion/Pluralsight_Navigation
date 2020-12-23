using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


/// <summary>
/// Custom property drawer for above attribute!
/// Based on code found here: http://answers.unity3d.com/questions/1034777/draw-scrptableobject-inspector-in-other-inspector.html
/// </summary>
[CustomPropertyDrawer(typeof(DisplayScriptableObjectPropertiesAttribute))]
public class DisplayScriptableObjectPropertiesDrawer : PropertyDrawer
{
	int lineHeight = 16;
	int drawerHeight = 20;
	int indentSize = 20;
	int HeaderDrawerHeight = 40;
	float DrawerHeight = 0;

	// Draw the property inside the given rect
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var e = Editor.CreateEditor(property.objectReferenceValue);
		var indent = EditorGUI.indentLevel;
		DrawerHeight = 0;
		position.height = lineHeight;
		EditorGUI.PropertyField(position, property);
		position.y += drawerHeight;
		if (e != null)
		{
			position.x += indentSize;
			position.width -= indentSize * 2;
			var so = e.serializedObject;
			so.Update();
			var prop = so.GetIterator();
			prop.NextVisible(true);
			int depthChilden = 0;
			bool showChilden = false;
			while (prop.NextVisible(true))
			{
				if (prop.depth == 0) { showChilden = false; depthChilden = 0; }
				if (showChilden && prop.depth > depthChilden)
				{
					continue;
				}
				float height = EditorGUI.GetPropertyHeight(prop);
				if (prop.isArray && prop.propertyType != SerializedPropertyType.String)
				{
					height = lineHeight;
				}

				position.height = height;
				EditorGUI.indentLevel = indent + prop.depth;
				if (EditorGUI.PropertyField(position, prop))
				{
					showChilden = false;
				}
				else
				{
					showChilden = true;
					depthChilden = prop.depth;
				}
				position.y += height + 2;
				this.DrawerHeight += height + 2;
			}

			if (GUI.changed)
			{
				so.ApplyModifiedProperties();
			}
		}
	}
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float height = base.GetPropertyHeight(property, label);
		height += DrawerHeight;
		return height;
	}
}