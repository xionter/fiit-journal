using FiitFlow.Domain;

namespace FiitFlow;

public class Extensions
{
    private IEnumerable<string> GetStandartSubjectsNames(int currentSemester)
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

}