using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public abstract class Graph <T>
{
	private List<Node<T>> nodes;
	private List<List<int>> distanceMatrix;

	public List<Node<T>> Nodes {
		get {
			return this.nodes;
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

	public List<List<int>> GetDistanceMatrix(){
		if (distanceMatrix == null || distanceMatrix.Count != nodes.Count) {
			distanceMatrix = new List<List<int>>();

			for (int i = 0; i < nodes.Count; i++) {
				distanceMatrix.Add(new List<int>());
			}

			for (int i = 0; i < nodes.Count; i++) {
				for (int j = i; j < nodes.Count; j++) {
					Node<T> node1 = nodes[i];
					Node<T> node2 = nodes[j];

					int distanceBetweenNodes = GetShortestDistanceBetween(node1.Value, node2.Value);
					distanceMatrix[i].Add(distanceBetweenNodes);

					if (i != j) {
						distanceMatrix[j].Add(distanceBetweenNodes);
					}
				}
			}
		}

		return distanceMatrix;
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

		if (startVertex == null || endVertex == null || startValue.Equals (endValue)) {
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
		
		if (startVertex == null || endVertex == null || startValue.Equals (endValue)) {
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
		pathTo.Add (startVertex, new List<T> ());
		pathTo [startVertex].Add (startVertex.Value);

		while (nodes.Count > 0) {
			Node<T> node = nodes.Dequeue ();

			if (visitedNodes.Contains (node)) {
			} else if (node.Equals (endVertex)) {
				pathTo [node].Add (node.Value);
				break;  
			} else { 
				visitedNodes.Add (node);

				foreach (var neighbour in node.Neighbours) {
					if (!pathTo.ContainsKey (neighbour)) {
						pathTo.Add (neighbour, new List<T> ());
					}
					pathTo [neighbour].AddRange (pathTo [node]);
					pathTo [neighbour].Add (neighbour.Value);
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

	public int GetShortestDistanceBetween(T node1, T node2){
		if (node1.Equals(node2)) {
			return 0;
		}

		return AStarPath (node1, node2).Count;	
	}

	public List<T> AStarPath (T start, T end, List<T> pathRestrictions=null)
	{
		if (pathRestrictions == null) {
			pathRestrictions = new List<T>();
		}

		Graph<T> graph = this;
		
		Node<T> startNode = graph.GetNode (start);
		HashSet<Node<T>> nodesEvaluated = new HashSet<Node<T>> ();
		HashSet<Node<T>> nodesToEvaluate = new HashSet<Node<T>> ();
		nodesToEvaluate.Add (startNode);
		
		Dictionary<T,T> cameFrom = new Dictionary<T,T> ();
		
		Dictionary<Node<T>, int> g_score = new Dictionary<Node<T>, int> ();
		g_score [startNode] = 0;
		
		Dictionary<Node<T>, int> f_score = new Dictionary<Node<T>, int> ();
		f_score [startNode] = g_score [startNode] + CalculateHeuristicCost (start, end);
		
		Node<T> endNode = graph.GetNode (end);
		
		while (nodesToEvaluate.Count > 0) {
			Node<T> current = GetNodeWithLowestCost (nodesToEvaluate, f_score);
			if (current.Equals (endNode)) {
				return ReconstructPath (cameFrom, end);
			}
			
			nodesToEvaluate.Remove (current);
			nodesEvaluated.Add (current);
			
			foreach (var neighbour in current.Neighbours) {
				if (pathRestrictions.Contains(neighbour.Value) && !neighbour.Value.Equals(end)) {
					nodesEvaluated.Add (neighbour);
					continue;
				}

				if (nodesEvaluated.Contains (neighbour)) {
					continue;
				}
				
				int neighbourGCost = g_score [current] + 1;
				
				if (!nodesToEvaluate.Contains (neighbour) || neighbourGCost < g_score [neighbour]) {
					cameFrom [neighbour.Value] = current.Value;
					g_score [neighbour] = neighbourGCost;
					f_score [neighbour] = g_score [neighbour] + CalculateHeuristicCost (neighbour.Value, end);
					
					if (!nodesToEvaluate.Contains (neighbour)) {
						nodesToEvaluate.Add (neighbour);
					}
				}
			}
		}
		
		return new List<T> ();
	}
	
	List<T> ReconstructPath (Dictionary<T, T> cameFrom, T end)
	{
		List<T> path = new List<T> ();
		path.Add (end);
		
		while (cameFrom.ContainsKey(end)) {
			end = cameFrom [end];
			path.Add (end);
		}
		
		return path;
	}

	protected abstract int CalculateHeuristicCost (T start, T end);

	private Node<T> GetNodeWithLowestCost (HashSet<Node<T>> nodesToEvaluate, Dictionary<Node<T>, int> f_score)
	{
		Node<T> nodeWithLowestScore = null;
		
		foreach (var node in nodesToEvaluate) {
			if (nodeWithLowestScore == null || f_score [node] < f_score [nodeWithLowestScore]) {
				nodeWithLowestScore = node;
			}
		}
		
		return nodeWithLowestScore;
	}

}
