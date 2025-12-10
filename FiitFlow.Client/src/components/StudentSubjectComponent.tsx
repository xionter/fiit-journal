import type StudentSubject from "./StudentSubject"
import type Student from "./Student"
import LoadingPageData from "./LoadingPageData"
import api from "./Api"

interface StudentSubjectProps {
    subjectName: string;
    student: Student;
    term: number;
    subjectReset: Function;

}

function StudentSubjectComponent({ subjectName, student, term, subjectReset }: StudentSubjectProps) {
    return (
        <LoadingPageData isLoading={false}>
            <a href="fiitflowmain.html" className="back-link">← Назад к списку предметов</a>

            <div className="subject-header">
                <h1 className="subject-title">Алгебра и геометрия</h1>
                <div className="subject-score-large">95</div>
            </div>

            <div className="progress-bar">
                <div className="progress-fill" style={{ width: "95%" }} ></div>
            </div>
            <div className="progress-text">
                <span>0 баллов</span>
                <span>100 баллов</span>
            </div>

            <h2 className="page-title">Детализация баллов</h2>

            <table className="details-table">
                <thead>
                    <tr>
                        <th>Тип работы</th>
                        <th>Название</th>
                        <th>Баллы</th>
                        <th>Максимум</th>
                        <th>Дата</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>Компьютерный практикум</td>
                        <td>Практикум 1</td>
                        <td className="score-good">7.8</td>
                        <td>8</td>
                        <td>12.03.2023</td>
                    </tr>
                    <tr>
                        <td>Компьютерный практикум</td>
                        <td>Практикум 2</td>
                        <td className="score-good">2</td>
                        <td>2</td>
                        <td>19.03.2023</td>
                    </tr>
                    <tr>
                        <td>Компьютерный практикум</td>
                        <td>Практикум 3</td>
                        <td className="score-good">1.8</td>
                        <td>2</td>
                        <td>26.03.2023</td>
                    </tr>
                    <tr>
                        <td>Компьютерный практикум</td>
                        <td>Практикум 4</td>
                        <td className="score-good">2</td>
                        <td>2</td>
                        <td>02.04.2023</td>
                    </tr>
                    <tr>
                        <td>Контрольные работы</td>
                        <td>Сумма контрольных</td>
                        <td className="score-medium">22.2</td>
                        <td>26</td>
                        <td>15.04.2023</td>
                    </tr>
                    <tr>
                        <td>Активность</td>
                        <td>Участие в семинарах</td>
                        <td className="score-medium">5</td>
                        <td>6</td>
                        <td>20.04.2023</td>
                    </tr>
                    <tr>
                        <td>Шеarn</td>
                        <td>Онлайн-курс</td>
                        <td className="score-good">5</td>
                        <td>5</td>
                        <td>25.04.2023</td>
                    </tr>
                    <tr>
                        <td>Экзамен</td>
                        <td>Итоговый экзамен</td>
                        <td className="score-good">55</td>
                        <td>55</td>
                        <td>10.05.2023</td>
                    </tr>
                </tbody>
            </table>

            <div className="formula-section">
                <h3 className="formula-title">Формула расчета итогового балла</h3>
                <div className="formula">
                    Итоговый балл = (Компьютерный практикум / 14 * 15) + (Контрольные работы / 26 * 20) +
                    (Активность / 6 * 5) + (Ulеarn / 5 * 5) + (Экзамен / 55 * 55)
                </div>
                <p>В текущем семестре: (13.6 / 14 * 15) + (22.2 / 26 * 20) + (5 / 6 * 5) + (5 / 5 * 5) + (55 / 55 * 55) = 95</p>
            </div>
        </LoadingPageData>
    );

    async function populateSubjectPointsDataByStudent() {
        api.get(`StudentSubjects/All`, {
            params: {
                id: student.id,
                firstName: student.firstName,
                lastName: student.lastName,
                group: student.group,
                term: term,
                time: Date.now(),
            }
        }).then(response => {
            if (response.status == 200) {
                const a = (response.data);
            }
        });
    }
}

export default StudentSubjectComponent;