using UInteractiveMusic.Runtime.Node;
using UnityEngine;

namespace UInteractiveMusic.Runtime {
    public class InteractiveMusicHierarchyAsset : ScriptableObject  
    {  
        [SerializeField] private MusicRootNode root;  
        public MusicRootNode Root => root;  

        public void SetRoot(MusicRootNode r) => root = r;  
    }  
}