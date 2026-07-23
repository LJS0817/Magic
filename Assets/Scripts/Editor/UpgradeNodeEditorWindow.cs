using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Magic.Upgrade;
using System.IO;

namespace Magic.Editor
{
    public class UpgradeNodeEditorWindow : EditorWindow
    {
        private List<UpgradeNodeSO> nodes;
        private Vector2 panOffset;
        private Vector2 dragOffset;

        private UpgradeNodeSO selectedNode;
        private UpgradeNodeSO linkingNode;
        private bool isPanning;

        private const float nodeWidth = 160f;
        private const float nodeHeight = 50f;

        [MenuItem("Magic/Upgrade Node Editor")]
        public static void ShowWindow()
        {
            UpgradeNodeEditorWindow window = GetWindow<UpgradeNodeEditorWindow>("Upgrade Editor");
            window.minSize = new Vector2(600, 400);
        }

        private void OnEnable()
        {
            LoadNodes();
        }

        private void LoadNodes()
        {
            nodes = new List<UpgradeNodeSO>();
            string[] guids = AssetDatabase.FindAssets("t:UpgradeNodeSO");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                UpgradeNodeSO node = AssetDatabase.LoadAssetAtPath<UpgradeNodeSO>(path);
                if (node != null)
                {
                    nodes.Add(node);
                }
            }
        }

        private void OnGUI()
        {
            DrawRadialGrid(70.505f, 0.2f, Color.gray);  
            DrawRadialGrid(141.42f, 0.4f, Color.gray); 

            DrawConnections();
            DrawConnectionLine(Event.current);
            DrawNodes();

            ProcessEvents(Event.current);

            if (GUI.changed) Repaint();
        }

        private void DrawRadialGrid(float spacing, float opacity, Color gridColor)
        {
            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, opacity);

            // 노드의 (0,0) 원점을 기준으로 원형 배경을 그립니다.
            Vector3 center = new Vector3(panOffset.x, panOffset.y, 0);

            // 화면 모서리까지의 최대 거리를 구해서 원을 몇 개 그릴지 결정
            float maxDist = 0f;
            Vector2[] corners = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(position.width, 0),
                new Vector2(0, position.height),
                new Vector2(position.width, position.height)
            };
            foreach (var corner in corners)
            {
                float dist = Vector2.Distance(center, corner);
                if (dist > maxDist) maxDist = dist;
            }

            int numCircles = Mathf.CeilToInt(maxDist / spacing);

            // 원 그리기
            for (int i = 1; i <= numCircles; i++)
            {
                Handles.DrawWireDisc(center, Vector3.forward, spacing * i);
            }

            // 추가로 중앙에서 뻗어나가는 선 8개를 그려줍니다 (방향 안내선)
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                Handles.DrawLine(center, center + dir * maxDist);
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        private void DrawNodes()
        {
            if (nodes == null) return;

            GUIStyle nodeStyle = new GUIStyle(GUI.skin.box);
            nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
            nodeStyle.border = new RectOffset(12, 12, 12, 12);
            nodeStyle.alignment = TextAnchor.MiddleCenter;
            nodeStyle.normal.textColor = Color.white;
            nodeStyle.fontStyle = FontStyle.Bold;

            GUIStyle selectedStyle = new GUIStyle(nodeStyle);
            selectedStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node == null) continue;

                Rect nodeRect = GetNodeRect(node);
                GUIStyle currentStyle = (selectedNode == node) ? selectedStyle : nodeStyle;

                GUI.Box(nodeRect, string.IsNullOrEmpty(node.nodeName) ? "New Node" : node.nodeName, currentStyle);

                // Add a small button or handle for output to make it intuitive
                Rect outputRect = new Rect(nodeRect.xMax - 15, nodeRect.center.y - 5, 10, 10);
                GUI.Box(outputRect, "", GUI.skin.button);
            }
        }

        private void DrawConnections()
        {
            if (nodes == null) return;

            foreach (var node in nodes)
            {
                if (node == null || node.requiredParents == null) continue;

                foreach (var parent in node.requiredParents)
                {
                    if (parent != null)
                    {
                        DrawBezier(parent, node);
                    }
                }
            }
        }

        private void DrawConnectionLine(Event e)
        {
            if (linkingNode != null)
            {
                Rect startRect = GetNodeRect(linkingNode);
                Vector2 startPos = new Vector2(startRect.xMax, startRect.center.y);
                Vector2 endPos = e.mousePosition;
                
                Handles.DrawBezier(
                    startPos,
                    endPos,
                    startPos + Vector2.right * 50f,
                    endPos + Vector2.left * 50f,
                    Color.white,
                    null,
                    2f
                );

                GUI.changed = true;
            }
        }

        private void DrawBezier(UpgradeNodeSO parent, UpgradeNodeSO child)
        {
            Rect startRect = GetNodeRect(parent);
            Rect endRect = GetNodeRect(child);

            Vector2 startPos = new Vector2(startRect.xMax, startRect.center.y);
            Vector2 endPos = new Vector2(endRect.xMin, endRect.center.y);

            Handles.DrawBezier(
                startPos,
                endPos,
                startPos + Vector2.right * 50f,
                endPos + Vector2.left * 50f,
                Color.cyan,
                null,
                2f
            );
        }

        private Rect GetNodeRect(UpgradeNodeSO node)
        {
            Vector2 screenPos = node.uiPosition + panOffset;
            return new Rect(screenPos.x - nodeWidth / 2, screenPos.y - nodeHeight / 2, nodeWidth, nodeHeight);
        }

        private void ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0) // Left click
                    {
                        bool clickedOnNode = false;
                        
                        // Check if clicking on output port
                        for (int i = nodes.Count - 1; i >= 0; i--)
                        {
                            var node = nodes[i];
                            if (node == null) continue;

                            Rect nodeRect = GetNodeRect(node);
                            Rect outputRect = new Rect(nodeRect.xMax - 20, nodeRect.center.y - 10, 20, 20);

                            if (outputRect.Contains(e.mousePosition))
                            {
                                linkingNode = node;
                                clickedOnNode = true;
                                e.Use();
                                break;
                            }
                            else if (nodeRect.Contains(e.mousePosition))
                            {
                                selectedNode = node;
                                dragOffset = e.mousePosition - node.uiPosition;
                                clickedOnNode = true;

                                // Double click to focus
                                if (e.clickCount == 2)
                                {
                                    EditorGUIUtility.PingObject(node);
                                    Selection.activeObject = node;
                                }

                                e.Use();
                                break;
                            }
                        }

                        if (!clickedOnNode)
                        {
                            selectedNode = null;
                            GUI.FocusControl(null);
                        }
                    }
                    else if (e.button == 1) // Right click
                    {
                        bool clickedOnNode = false;
                        for (int i = nodes.Count - 1; i >= 0; i--)
                        {
                            var node = nodes[i];
                            if (node != null && GetNodeRect(node).Contains(e.mousePosition))
                            {
                                selectedNode = node;
                                clickedOnNode = true;
                                ProcessNodeContextMenu(e, node);
                                break;
                            }
                        }

                        if (!clickedOnNode)
                        {
                            ProcessContextMenu(e);
                        }
                    }
                    else if (e.button == 2 || (e.button == 0 && e.alt)) // Middle click or Alt+Left
                    {
                        isPanning = true;
                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (linkingNode != null)
                    {
                        for (int i = 0; i < nodes.Count; i++)
                        {
                            var targetNode = nodes[i];
                            if (targetNode != null && targetNode != linkingNode)
                            {
                                if (GetNodeRect(targetNode).Contains(e.mousePosition))
                                {
                                    AddParentToNode(linkingNode, targetNode);
                                    break;
                                }
                            }
                        }
                        linkingNode = null;
                        e.Use();
                    }
                    isPanning = false;
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0 && selectedNode != null && linkingNode == null && !isPanning)
                    {
                        Undo.RecordObject(selectedNode, "Move Upgrade Node");
                        selectedNode.uiPosition = e.mousePosition - dragOffset;
                        EditorUtility.SetDirty(selectedNode);
                        e.Use();
                    }
                    else if (isPanning)
                    {
                        panOffset += e.delta;
                        e.Use();
                    }
                    break;
            }
        }

        private void AddParentToNode(UpgradeNodeSO parent, UpgradeNodeSO child)
        {
            if (child.requiredParents == null) child.requiredParents = new List<UpgradeNodeSO>();
            
            // Avoid cycles (simple check) or duplicates
            if (!child.requiredParents.Contains(parent) && !IsAncestor(child, parent))
            {
                Undo.RecordObject(child, "Add Required Parent");
                child.requiredParents.Add(parent);
                EditorUtility.SetDirty(child);
            }
        }

        private bool IsAncestor(UpgradeNodeSO potentialAncestor, UpgradeNodeSO node)
        {
            if (node.requiredParents == null) return false;
            if (node.requiredParents.Contains(potentialAncestor)) return true;
            
            foreach (var parent in node.requiredParents)
            {
                if (parent != null && IsAncestor(potentialAncestor, parent))
                {
                    return true;
                }
            }
            return false;
        }

        private void ProcessContextMenu(Event e)
        {
            GenericMenu genericMenu = new GenericMenu();
            Vector2 mousePos = e.mousePosition;
            genericMenu.AddItem(new GUIContent("Create New Node"), false, () => CreateNewNode(mousePos));
            genericMenu.ShowAsContext();
            e.Use();
        }

        private void ProcessNodeContextMenu(Event e, UpgradeNodeSO node)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Clear Parents"), false, () => {
                Undo.RecordObject(node, "Clear Parents");
                node.requiredParents.Clear();
                EditorUtility.SetDirty(node);
            });
            genericMenu.AddSeparator("");
            genericMenu.AddItem(new GUIContent("Delete Node"), false, () => {
                DeleteNode(node);
            });
            genericMenu.ShowAsContext();
            e.Use();
        }

        private void CreateNewNode(Vector2 mousePosition)
        {
            string folderPath = "Assets/Resources/Upgrades";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string[] parts = folderPath.Split('/');
                string currentPath = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    if (!AssetDatabase.IsValidFolder(currentPath + "/" + parts[i]))
                    {
                        AssetDatabase.CreateFolder(currentPath, parts[i]);
                    }
                    currentPath += "/" + parts[i];
                }
            }

            UpgradeNodeSO newNode = CreateInstance<UpgradeNodeSO>();
            newNode.nodeID = System.Guid.NewGuid().ToString();
            newNode.nodeName = "New Upgrade";
            newNode.uiPosition = mousePosition - panOffset;

            // Generate unique filename
            string fullPath = $"{folderPath}/Node_{System.Guid.NewGuid().ToString().Substring(0, 8)}.asset";
            AssetDatabase.CreateAsset(newNode, fullPath);
            AssetDatabase.SaveAssets();

            nodes.Add(newNode);
        }

        private void DeleteNode(UpgradeNodeSO node)
        {
            string path = AssetDatabase.GetAssetPath(node);
            
            // Remove connections to this node
            foreach (var n in nodes)
            {
                if (n != null && n.requiredParents != null && n.requiredParents.Contains(node))
                {
                    n.requiredParents.Remove(node);
                    EditorUtility.SetDirty(n);
                }
            }

            nodes.Remove(node);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
        }
    }
}
