/**
  * @file :InteractiveMusicHierarchyAsset
  * @brief :BRIEF
  * @details : Detail of this file
  * @author :六队
  * @version :0.1
  * @date :2025年9月4日
  * @copyright :Foeye
 */

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