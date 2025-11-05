using System.Collections.Generic;
using System;


namespace FiitFlow.Domain;

public class Student
{
    public string Name  { get; }
    public string GroupId { get; } //фт-201-2 и тд, чтобы можно было брать уже имеющиеся таблички

    public readonly Guid Guid = Guid.NewGuid();
    public int CourseYear { get; }
    public int CurrentSemester { get; }

    // вообще можно попробовать подгружать инфу про группу и курс из фиитобота))
    // чтобы как артем и говорил сразу после логина все работало
    
    public IEnumerable<Subject>? Subjects { get; private set; } // будем хранить список выбранных предметов? или по курсу брать сразу все
    
    public Student(string name, int courseCount, string groupId)
    {
        Name = name;
        CourseYear = courseCount;
        GroupId = groupId;
        CurrentSemester = DateTime.Now.Month >= 9 ? courseCount * 2 - 1: courseCount * 2; // если месяц с сентября по февраль - 1 семестр, иначе 2 семестр
        
        //GetStandartSubjectsNames();
        
        // тут какой-нибудь метод чтобы пользователь выбрал из предложенных предметов нужные ему
        // и заполнил ссылки на таблички
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
    
    public void GetPoints()
    {
        // ну тут парсер чет делает
        // типо parser.Parse(linkToSheet)
        throw new NotImplementedException();
    }
}