using FiitFlow.Domain;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FiitFlow;

public static class Extensions
{
    private static IEnumerable<string> GetStandartSubjectsNames(int currentSemester)
    {
        return currentSemester switch
        {
            1 => SemesterSubjects.Sem1Subjects,
            2 => SemesterSubjects.Sem2Subjects,
            3 => SemesterSubjects.Sem3Subjects,
            4 => SemesterSubjects.Sem4Subjects,
            _ => throw new NotImplementedException()
        };
    }

    public static int GetCurrentSemester(this GroupEntity group)
    {
        var currentDate = DateTime.Now;
        
        var enrollmentYear = Int32.Parse(group.GroupTitle[3].ToString());
        //Console.WriteLine(enrollmentYear);
        var semester = enrollmentYear * 2;
        if (currentDate.Month >= 9 || currentDate.Month <= 2)
        {
            semester -= 1;
        }
        else
        {
            semester += 1;
        }
        return semester;
    }
    public static (string firstName, string lastName) SplitStudentName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return (string.Empty, string.Empty);

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
            return (parts[0], string.Empty);

        var lastName = parts[0] + " ";
        var firstName = string.Join(' ', parts.Skip(1));
        return (firstName, lastName);
    }
}