using System.Reflection;
using UnityEditor;

namespace HongHeng.UnityReferenceTracker.Core {

    public static class MissingReferenceTracker {

        public static ReferenceFile FindMissingReferenceFiles(string path) {
            return RefApi.GetReference(path, IsMissingProperty, true);
        }

        private static bool IsMissingProperty(SerializedProperty sp) {
            //sp.objectReferenceInstanceIDValue != 0// cannot work from Unity 2018.3
            return sp.propertyType == SerializedPropertyType.ObjectReference
                   && sp.objectReferenceValue == null
                   && ((string) sp.GetType().InvokeMember("objectReferenceStringValue",
                       BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty,
                       null, sp, new object[0])).StartsWith("Missing");
        }

    }

}
