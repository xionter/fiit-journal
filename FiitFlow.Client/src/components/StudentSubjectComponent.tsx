import type StudentSubject from "./StudentSubject"
import LoadingPageData from "./LoadingPageData"
import api from "./Api"
import { useEffect, useState } from "react"
import { Link } from "react-router-dom";

interface SubjectTableResult {
    studentName: string;
    tableName: string;
    tableUrl: string;
    sheetName: string;
    data: Record<string, string>;
}

interface StudentSubjectResult {
    studentName: string;
    tables: SubjectTableResult[];
}

function StudentSubjectComponent({ subjectName, student, term, score }: StudentSubject) {
    const [subjectInfo, setSubjectInfo] = useState<StudentSubjectResult>();

    useEffect(() => {
        populateSubjectPointsDataByStudent();
    }, []);

    return (
        <LoadingPageData isLoading={subjectInfo === undefined}>
            <div className="subject-header">
                <h1 className="subject-title">{subjectName}</h1>
                <div className="subject-score-large">{score}</div>
            </div>

            <div className="progress-bar">
                <div className="progress-fill" style={{ width: `${score}%` }} ></div>
            </div>
            <div className="progress-text">
                <span>0 баллов</span>
                <span>100 баллов</span>
            </div>

            <h2 className="page-title">Детализация</h2>

            <table className="details-table">
                <thead>
                    <tr>
                        <th>Лист</th>
                        <th>Столбец</th>
                        <th>Данные</th>
                    </tr>
                </thead>
                <tbody>
                    {subjectInfo?.tables.map((table, tIndex) => Object.entries(table.data).map(([tag, val], index) => (
                        <tr key={tIndex * 100 + index}>
                            <td>{table.sheetName}</td>
                            <td>{tag}</td>
                            <td>{val}</td>
                        </tr>
                    )))}
                </tbody>
            </table>

            <div className="formula-section">
                <h3 className="formula-title">Формула</h3>
                <div className="formula">Формула будет добавлена позже</div>
                <p>В текущем семестре: {score}</p>
            </div>
            <div className="formula-section">
                <h3 className="formula-title">Исходная таблица</h3>
                {subjectInfo?.tables.length !== undefined && subjectInfo?.tables.length > 0 ?
                    <div className="formula"><a href={subjectInfo?.tables[0].tableUrl}>{subjectInfo?.tables[0].tableUrl}</a></div> :
                    <div></div>
                }
            </div>
        </LoadingPageData>
    );

    async function populateSubjectPointsDataByStudent() {
        api.get(`StudentSubjects/SubjectInfo`, {
            params: {
                id: student.id,
                firstName: student.firstName,
                lastName: student.lastName,
                group: student.group,
                term: term,
                subjectName: subjectName,
                time: Date.now(),
            }
        }).then(response => {
            if (response.status == 200) {
                setSubjectInfo(response.data);
            }
        });
    }
}

export default StudentSubjectComponent;