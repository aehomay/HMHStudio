using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicStudio.Core.Model.DataStructure
{
    public struct MatrixSize
    {
        public int X;
        public int Y;
    }

    public class Matrix<T>:ICloneable
    {
        T[,] _matrix;
        MatrixSize _size;

        public Matrix() { }
        public Matrix(int x, int y)
        {
            MatrixSize size;
            size.X = x;
            size.Y = y;
            Size = size;
        }

        public Matrix(MatrixSize size)
        {
            Size = size;
        }

        public int Count
        {
            get
            {
                return _matrix.Length;
            }
        }

        public MatrixSize Size
        {
            get
            {
                return _size;
            }

            set
            {
                if (value.X < 1 || value.Y < 1)
                    throw new Exception("Size of matrix can not be less than 1x1.");
                _size = value;
                _matrix = new T[_size.X, _size.Y];
            }
        }

        public void Write(T item, int x, int y)
        {
            if ((x > (_size.X - 1) || y > (_size.Y - 1)) || (x < 0 || y < 0))
                throw new NullReferenceException();
            _matrix[x, y] = item;
        }

        public T Read(int x, int y)
        {
            if ((x > (_size.X - 1) || y > (_size.Y - 1)) || (x < 0 || y < 0))
                throw new NullReferenceException();

            return _matrix[x, y];
        }

        public T[] Read(int x)
        {
            if (x > _size.X - 1 || x < 0 )
                throw new NullReferenceException();
            T[] row = new T[Size.Y];
            for (int i = 0; i < Size.Y; i++)
                row[i] = _matrix[x, i];
            return row;
        }

        public void ReSize(int x, int y)
        {
            T[,] matrix = new T[x, y];

            for (int i = 0; i < _size.X; i++)
                for (int j = 0; j < _size.Y; j++)
                    matrix[i, j] = _matrix[i, j];

            _size = new MatrixSize() { X = x, Y = y };
            _matrix = matrix;
        }

        public void Clear()
        {
            Size = _size;
        }

        public object Clone()
        {

            Matrix<T> matrix = new DataStructure.Matrix<T>(Size);
            for (int i = 0; i < _size.X; i++)
            {
                for (int j = 0; j < _size.Y; j++)
                {
                    matrix.Write(_matrix[i, j],i, j);
                }
            }
            return matrix;
        }
    }
}
