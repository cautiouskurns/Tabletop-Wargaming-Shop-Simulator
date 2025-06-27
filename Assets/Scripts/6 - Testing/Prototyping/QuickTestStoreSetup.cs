using UnityEngine;

namespace TabletopShop
{
    public class QuickTestStoreSetup : MonoBehaviour
    {
        [ContextMenu("Create Test Store")]
        public void CreateTestStore()
        {
            // Create floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.localScale = new Vector3(2, 1, 2);

            // Create 3 test shelves
            for (int i = 0; i < 3; i++)
            {
                GameObject shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shelf.name = $"TestShelf_{i + 1:D2}";
                shelf.transform.position = new Vector3((i - 1) * 4f, 0.5f, 5f);
                shelf.transform.localScale = new Vector3(2f, 1f, 0.5f);

                // Add test shelf script
                shelf.AddComponent<TestShelfSlot>();

                // Random color
                var renderer = shelf.GetComponent<Renderer>();
                var material = new Material(Shader.Find("Standard"));
                material.color = new Color(Random.value, Random.value, Random.value, 1f);
                renderer.material = material;
            }

            Debug.Log("Test store created! Don't forget to bake NavMesh!");
        }
    }
}