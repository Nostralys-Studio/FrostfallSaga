using System;
using System.Collections.Generic;
using System.Linq;
using FrostfallSaga.Core.Dialogues;
using FrostfallSaga.Utils.DataStructures.TreeNode;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrostfallSaga.FFSEditor.DialogueSystem
{
    public class DialogueGraphView : GraphView
    {
        private static readonly float NODE_WIDTH = 250;
        private static readonly float NODE_HEIGHT = 150;
        private static readonly float HORIZONTAL_SPACING = 500;
        private static readonly float VERTICAL_SPACING = 300;
        private DialogueSO _currentDialogue;

        public DialogueGraphView()
        {
            style.flexGrow = 1;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            GridBackground grid = new();
            Insert(0, grid);
            grid.StretchToParentSize();
        }

        public void PopulateFromDialogue(DialogueSO dialogue)
        {
            _currentDialogue = dialogue;
            ClearGraph();

            if (dialogue.DialogueTreeRoot == null || dialogue.DialogueTreeRoot.Data == null)
            {
                DialogueLine rootData = new("Start", "Begin conversation...", null, false);
                dialogue.SetRoot(new TreeNode<DialogueLine>(Guid.NewGuid().ToString(), rootData));
                AutoSave();
            }

            DialogueNode rootNode = CreateGraphNodeTree(dialogue.DialogueTreeRoot, 0, null);
            rootNode.SetPosition(new Rect(500, 100, NODE_WIDTH, NODE_HEIGHT));
        }

        private DialogueNode CreateGraphNodeTree(TreeNode<DialogueLine> treeNode, int depth, DialogueNode parentNode)
        {
            DialogueNode graphNode = CreateDialogueNode(treeNode, depth, parentNode);
            foreach (TreeNode<DialogueLine> child in treeNode.Children)
                CreateGraphNodeTree(child, depth + 1, graphNode);

            return graphNode;
        }

        private DialogueNode CreateDialogueNode(TreeNode<DialogueLine> treeNode, int depth, DialogueNode parentNode)
        {
            DialogueNode newNode = new(treeNode, depth, parentNode);
            newNode.OnAnswerAdded += HandleAnswerAdded;
            newNode.OnAddLineContinuation += HandleContinuationAdded;
            newNode.OnNodeRemoved += HandleNodeRemoved;
            newNode.OnNodeModified += AutoSave;
            AddElement(newNode);

            if (parentNode != null)
            {
                ConnectNodes(parentNode, newNode);
                parentNode.AddChild(newNode);
                AutoPositionChildren(parentNode);
            }

            return newNode;
        }

        private void AutoPositionChildren(DialogueNode parentNode)
        {
            List<DialogueNode> children = parentNode.ChildrenNodes;
            if (children.Count == 0) return;

            Rect parentRect = parentNode.GetPosition();
            float startX = parentRect.x - (children.Count - 1) * HORIZONTAL_SPACING / 2;
            float newY = parentRect.y + (VERTICAL_SPACING * parentNode.Depth + 1);

            for (int i = 0; i < children.Count; i++)
            {
                DialogueNode childNode = children[i];
                float newX = startX + i * HORIZONTAL_SPACING;
                childNode.SetPosition(new Rect(newX, newY, NODE_WIDTH, NODE_HEIGHT));
            }
        }

        private void HandleAnswerAdded(DialogueNode parentNode)
        {
            DialogueLine newLine = new("New Answer", "Enter text...", null, false);
            TreeNode<DialogueLine> newTreeNode = new(Guid.NewGuid().ToString(), newLine);
            parentNode.TreeNode.AddChild(newTreeNode);
            parentNode.TreeNode.Data.SetAnswers(
                parentNode.TreeNode.Children.Select(child => child.Data.Title).ToArray()
            );

            CreateDialogueNode(newTreeNode, parentNode.Depth + 1, parentNode);
            parentNode.RefreshNode();
            AutoSave();
        }

        private void HandleContinuationAdded(DialogueNode parentNode)
        {
            DialogueLine newLine = new("Continuation", "Continue text...", null, false);
            TreeNode<DialogueLine> newTreeNode = new(Guid.NewGuid().ToString(), newLine);
            parentNode.TreeNode.AddChild(newTreeNode);

            DialogueNode newNode = CreateDialogueNode(newTreeNode, parentNode.Depth + 1, parentNode);
            parentNode.RefreshNode();
            AutoSave();
        }

        private void HandleNodeRemoved(DialogueNode nodeToRemove)
        {
            DialogueNode parentNode = nodeToRemove.ParentNode;
            TreeNode<DialogueLine> parentTreeNode = nodeToRemove.TreeNode.Parent;
            if (parentTreeNode == null) return;

            int index = parentTreeNode.Children.IndexOf(nodeToRemove.TreeNode);
            parentTreeNode.RemoveChild(nodeToRemove.TreeNode);
            DialogueLine parentData = parentTreeNode.Data;

            if (parentData.Answers != null && parentData.Answers.Length > 0 && index < parentData.Answers.Length)
            {
                List<string> answersList = new(parentData.Answers);
                answersList.RemoveAt(index);
                parentData.SetAnswers(answersList.ToArray());

                if (nodeToRemove.InputPort.connections.FirstOrDefault()?.output?.node is DialogueNode parentOfRemoved)
                    parentOfRemoved.RefreshNode();
            }

            RemoveNodeAndChildrenInGraph(nodeToRemove);
            AutoPositionChildren(parentNode);
            AutoSave();
        }

        private void RemoveNodeAndChildrenInGraph(DialogueNode nodeToRemove)
        {
            List<DialogueNode> childrenToRemove = new();

            foreach (Edge edge in nodeToRemove.OutputPort.connections.ToList())
            {
                if (edge.input.node is DialogueNode childNode)
                    childrenToRemove.Add(childNode);
                RemoveElement(edge);
            }

            foreach (DialogueNode child in childrenToRemove)
                RemoveNodeAndChildrenInGraph(child);

            if (nodeToRemove.InputPort.connections.Any())
                RemoveElement(nodeToRemove.InputPort.connections.First());

            RemoveElement(nodeToRemove);
        }

        private void ConnectNodes(DialogueNode parentNode, DialogueNode childNode)
        {
            if (parentNode == null || childNode == null) return;

            Edge edge = parentNode.OutputPort.ConnectTo(childNode.InputPort);
            AddElement(edge);
        }

        public void ClearGraph()
        {
            foreach (GraphElement element in graphElements.ToList())
                RemoveElement(element);
        }

        private void AutoSave()
        {
            if (_currentDialogue == null) return;
            _currentDialogue.SaveTree(); // <-- Serialize to DTOs
            EditorUtility.SetDirty(_currentDialogue);
        }
    }
}
