//------------------------------------------------------------------------------
// <auto-generated>
//     O código foi gerado por uma ferramenta.
//     Versão de Tempo de Execução:4.0.30319.34014
//
//     As alterações ao arquivo poderão causar comportamento incorreto e serão perdidas se
//     o código for gerado novamente.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using UnityEngine;

public class Int32Point {
	public Int32Point(int x, int y) 
	{
		X = x;
		Y = y;
	}
	
	public int X { get; private set; }
	public int Y { get; private set; }

	public override bool Equals (object obj)
	{
		if (obj is Int32Point) {
			Int32Point otherPoint = (Int32Point) obj;

			return this.X == otherPoint.X && this.Y == otherPoint.Y;
		}

		return false;
	}

	public Vector3 ToVector3(){
		return new Vector3 (X, Y, 0);
	}

	public override string ToString ()
	{
		return string.Format ("[X={0}, Y={1}]", X, Y);
	}

	public static Int32Point operator +(Int32Point point1, Int32Point point2){
		return new Int32Point (point1.X + point2.X, point1.Y + point2.Y);
	}

	public static Int32Point operator -(Int32Point point1, Int32Point point2){
		return new Int32Point (point1.X - point2.X, point1.Y - point2.Y);
	}

	public static Int32Point GenerateRandomPoint(){
		return GenerateRandomPoint (int.MinValue, int.MinValue, int.MaxValue, int.MaxValue);
	}

	public static Int32Point GenerateRandomPoint(int minX, int minY, int maxX, int maxY){
		return new Int32Point (UnityEngine.Random.Range (minX, maxX), UnityEngine.Random.Range (minY, maxY));
	}

	public override int GetHashCode ()
	{
		return X.GetHashCode () + Y.GetHashCode ();
	}
}

