using System.Collections.Generic;
using System;


namespace FiitFlow.Domain;

public class Student
{
    public readonly string Name;
    public readonly string GroupId; //фт-201-2 и тд, чтобы можно было брать уже имеющиеся таблички

    public readonly int CourseCount;
    public readonly int SemesterCount;
    
    // вообще можно попробовать подгружать инфу про группу и курс из фиитобота))
    // чтобы как артем и говорил сразу после логина все работало
    
    public IEnumerable<string>? Subjects { get; private set; } // будем хранить список выбранных предметов? или по курсу брать сразу все
    
    public Student(string name, int courseCount, string groupId)
    {
        Name = name;
        CourseCount = courseCount;
        GroupId = groupId;
        SemesterCount = DateTime.Now.Month >= 9 ? courseCount * 2 - 1: courseCount * 2; // если месяц с сентября по февраль - 1 семестр, иначе 2 семестр
        
        GetStandartSubjectsNames();
        
        // тут какой нибудь метод чтобы пользователь выбрал из предложенных предметов нужные ему
        // и заполнил ссылки на таблички
    }

    private IEnumerable<string> GetStandartSubjectsNames()
    {
        return SemesterCount switch
        {
            1 => SemesterSubjects.Sem1Subjects,
            //2 => SemesterSubjects.Sem2Subjects,
            //3 => SemesterSubjects.Sem3Subjects,
            //4 => SemesterSubjects.Sem4Subjects,
            _ => throw new NotImplementedException()
        };
    }
    
    public void AddSubject(string subjectTitle)
    {
        // ну тут добавить предмет в список выбранных
        throw new NotImplementedException();
    }
    
    public void DeleteSubject(string subjectTitle)
    {
        // ну тут удалить предмет из списка выбранных
        throw new NotImplementedException();
    }
}