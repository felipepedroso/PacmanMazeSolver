using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Node<T>{
	public T Value { get; private set; }
	public List<Node<T>> Neighbours{ get; private set; }

	public Node(T value){
		Value = value;
		Neighbours = new List<Node<T>> ();
	}

	public void AddNeighbour(Node<T> node){
		if (node != null) {
			if (!Neighbours.Contains(node)) {
				Neighbours.Add(node);
			}
		}
	}

	public override bool Equals (object obj)
	{
		if (obj is Node<T>) {
			Node<T> otherNode = (Node<T>) obj;

			return this.Value.Equals(otherNode.Value);
		}

		return false;
	}
}


