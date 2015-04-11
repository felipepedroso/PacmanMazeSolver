using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class Graph <T>
{
	private List<Node<T>> nodes;

	public Node<T>[] Nodes {
		get {
			return this.nodes.ToArray ();
		}
	}

	public Graph ()
	{
		this.nodes = new List<Node<T>> ();
	}

	public Node<T> GetNode (T value)
	{
		foreach (Node<T> node in nodes) {
			if (node.Value.Equals (value)) {
				return node;
			}
		}

		return null;
	}

	private void AddNode (Node<T> node)
	{
		if (node != null && !nodes.Contains (node)) {
			nodes.Add (node);
		}
	}

	private void RemoveNode (Node<T> node)
	{
		if (node != null && nodes.Contains (node)) {
			nodes.Remove (node);
		}
	}

	public void AddEdge (T value1, T value2)
	{
		if (value1 == null || value2 == null) {
			return;
		}

		Node<T> node1 = GetNode (value1), node2 = GetNode (value2);

		if (node1 == null) {
			node1 = new Node<T> (value1);
			AddNode (node1);
		}

		if (node2 == null) {
			node2 = new Node<T> (value2);
			AddNode (node2);
		}

		node1.AddNeighbour (node2);
		node2.AddNeighbour (node1);
	}

	public virtual List<T> DepthFirstSearchPath (T startValue, T endValue)
	{
		Node<T> startVertex = GetNode (startValue);
		Node<T> endVertex = GetNode (endValue);

		if (startVertex == null || endVertex == null || startValue.Equals(endValue)) {
			return new List<T> ();
		}

		return DepthFirstSearchPath (startVertex, endVertex, new List<Node<T>> (), new List<T> ());
	}

	private List<T> DepthFirstSearchPath (Node<T> startVertex, Node<T> endVertex, List<Node<T>> visitedNodes, List<T> pathSoFar)
	{
		//PrintInteraction(startVertex, endVertex, visitedNodes);
		visitedNodes.Add (startVertex);
		pathSoFar.Add (startVertex.Value);

		if (startVertex.Equals (endVertex)) {
			pathSoFar.Add (startVertex.Value);
			return pathSoFar;
		}


		foreach (var neighbour in startVertex.Neighbours) {
			if (!visitedNodes.Contains (neighbour)) {

				List<T> neighbourPath = DepthFirstSearchPath (neighbour, endVertex, visitedNodes, pathSoFar);
				if (neighbourPath.Count > 0) {
					return neighbourPath;
				} else {
					pathSoFar.Add (startVertex.Value);					
				}
			}
		}
	
		return new List<T> ();
	}

	void PrintInteraction (Node<T> startVertex, Node<T> endVertex, List<Node<T>> visitedNodes)
	{
		StringBuilder strBuilder = new StringBuilder ();
		strBuilder.AppendFormat ("\nStart Vertex: {0}", startVertex.Value);
		strBuilder.AppendFormat ("\nEnd Vertex: {0}", endVertex.Value);
		strBuilder.AppendFormat ("\nVisited Nodes ({0}): ", visitedNodes.Count);
		foreach (var node in visitedNodes) {
			strBuilder.AppendFormat ("\n   {0}", node.Value);
		}
		Debug.Log (strBuilder.ToString ());
	}

	public virtual List<T> BreadthFirstSearchShortestPath (T startValue, T endValue)
	{
		Node<T> startVertex = GetNode (startValue);
		Node<T> endVertex = GetNode (endValue);
		
		if (startVertex == null || endVertex == null || startValue.Equals(endValue)) {
			return new List<T> ();
		}
		
		return BreadthFirstSearchShortestPath (startVertex, endVertex);
	}

	private List<T> BreadthFirstSearchShortestPath (Node<T> startVertex, Node<T> endVertex)
	{
		Queue<Node<T>> nodes = new Queue<Node<T>> ();
		List<Node<T>> visitedNodes = new List<Node<T>> ();
		Dictionary<Node<T>,List<T>> pathTo = new Dictionary<Node<T>, List<T>> ();
	
		nodes.Enqueue (startVertex);
		pathTo.Add(startVertex, new List<T>());
		pathTo [startVertex].Add (startVertex.Value);

		while (nodes.Count > 0) {
			Node<T> node = nodes.Dequeue ();

			if (visitedNodes.Contains (node)) {
			} else if (node.Equals (endVertex)) {
				pathTo[node].Add(node.Value);
				break;  
			} else { 
				visitedNodes.Add (node);

				foreach (var neighbour in node.Neighbours) {
					if (!pathTo.ContainsKey(neighbour)) {
						pathTo.Add(neighbour, new List<T>());
					}
					pathTo[neighbour].AddRange(pathTo[node]);
					pathTo[neighbour].Add(neighbour.Value);
					nodes.Enqueue (neighbour);
				}
			} 
		} 

		return pathTo [endVertex];
	}

	public override string ToString ()
	{
		StringBuilder stringBuilder = new StringBuilder ("");

		foreach (Node<T> node in nodes) {
			stringBuilder.Append (node.Value);

			foreach (Node<T> neighbour in node.Neighbours) {
				stringBuilder.Append (" => " + neighbour.Value);
			}

			stringBuilder.Append ("\n");
		}

		return stringBuilder.ToString ();
	}
}
