using FrostfallSaga.Utils.Editor;
using FrostfallSaga.Utils.GameObjectVisuals.GameObjectSpawnControllers;
using UnityEditor;

namespace FrostfallSaga.FFSEditor.GameObjectVisuals
{
    [CustomPropertyDrawer(typeof(AGameObjectSpawnController), true)]
    public class AGameObjectSpawnControllerDrawer : AbstractAttributeDrawer<AGameObjectSpawnController>
    {
    }
}