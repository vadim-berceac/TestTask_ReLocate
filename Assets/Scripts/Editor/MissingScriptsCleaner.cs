using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class MissingScriptsCleaner : EditorWindow
{
    [MenuItem("Tools/Remove Missing Scripts from Scene")]
    public static void RemoveMissingScriptsFromScene()
    {
        var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        var totalRemoved = 0;

        foreach (var rootObj in rootObjects)
        {
            var allChildren = rootObj.GetComponentsInChildren<Transform>(true);
            
            foreach (var child in allChildren)
            {
                var count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(child.gameObject);
                if (count > 0)
                {
                    totalRemoved += count;
                    Debug.Log($"[Cleaner] Удалено {count} missing-скриптов с объекта: {child.name}", child.gameObject);
                }
            }
        }

        if (totalRemoved > 0)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"<b>[Cleaner] Успешно!</b> Всего удалено сломанных скриптов: {totalRemoved}");
        }
        else
        {
            Debug.Log("[Cleaner] Проверка завершена. Пропущенных или сломанных скриптов на сцене не найдено.");
        }
    }
}