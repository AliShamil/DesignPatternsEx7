using System.Collections;

namespace İteratorPattern;

#nullable disable


public class Student
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }

    public override string ToString()
    => @$"ID: {Id}
Name: {Name}
Surname: {Surname}";


}


public class StudentCollection : IIterable<Student>
{
    private readonly List<Student> _students = new();
    public int Count => _students.Count;

    public void Add(Student student) => _students.Add(student);

    public Student this[int index]
    {
        get
        {
            return _students[index];
        }
        set
        {
            _students[index] = value;
        }
    }

    public IIterator<Student> GetEnumerator() => new StudentIterator(this);

}

class StudentIterator : IIterator<Student>
{
    private int currIndex = -1;

    private StudentCollection _students;

    public StudentIterator(StudentCollection students) => _students = students;

    public Student Current => _students[currIndex];

    public bool MoveNext() => ++currIndex < _students.Count;

    public void Reset() => currIndex = -1;
}

public interface IIterator<T>
{
    T Current { get; }
    bool MoveNext();
    void Reset();
}

public interface IIterable<T>
{
    IIterator<T> GetEnumerator();
}




class Program
{

    static void Main()
    {
        StudentCollection students = new();
        students.Add(new Student { Id = 1, Name = "Niko", Surname = "Akremi" });
        students.Add(new Student { Id = 2, Name = "Kenan", Surname = "Muradov" });
        students.Add(new Student { Id = 3, Name = "Samir", Surname = "Purrengi" });



        foreach (var item in students)
        {
            Console.WriteLine(item);
        }

        students[2]= new() { Id = 15, Name= "Ali", Surname = "Shamilzade" };
        Console.WriteLine();

        foreach (var item in students)
        {
            Console.WriteLine(item);
        }
    }
}