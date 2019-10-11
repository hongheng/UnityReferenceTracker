using UnityEditor;

namespace HongHeng.UnityReferenceTracker {

    public class ReferenceProperty {

        //public SerializedProperty Property;
        public readonly string PropertyPath;
        public readonly string ReferenceValue;

        public ReferenceProperty(SerializedProperty sp) {
            //Property = sp.Copy(),
            PropertyPath = sp.propertyPath;
            ReferenceValue = sp.objectReferenceValue != null
                ? $"{sp.objectReferenceValue.name} ({sp.objectReferenceValue.GetType()})"
                : null;
        }

        public override string ToString() {
            return $"{PropertyPath} = {ReferenceValue}";
        }

    }

}