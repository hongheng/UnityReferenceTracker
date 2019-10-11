using System.Collections.Generic;

namespace HongHeng.UnityReferenceTracker {

    public class ReferenceObject {

        //public Object ReferenceObj;
        public readonly string ReferenceObjInfo;
        public List<ReferenceProperty> ReferenceProperties = new List<ReferenceProperty>();

        public ReferenceObject(string referenceObjInfo) {
            ReferenceObjInfo = referenceObjInfo;
        }

        public static readonly ReferenceObject Empty = new ReferenceObject("Empty Object");

        public static readonly ReferenceObject EmptyGameObject =
            new ReferenceObject("Empty GameObject");

        public static ReferenceObject EmptyComponent(string gameObjectInfo) {
            return new ReferenceObject($"Empty Component in [GameObject:{gameObjectInfo}]");
        }

    }

}