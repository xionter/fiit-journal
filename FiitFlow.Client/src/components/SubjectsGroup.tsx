import { Fragment, useEffect, useState } from "react"
import LoadingPageData from "./LoadingPageData"
import type Student from "./Student"
import type SubjectItem from "./SubjectItem"
import api from "./Api"
import type StudentSubject from "./StudentSubject"
import { loadSubjectCookie, saveSubjectCookie, removeSubjectCookie } from "./CookieTools"
import StudentSubjectComponent from "./StudentSubjectComponent"

interface SubjectGroupProps {
    student: Student;
    term: number;
}

function SubjectsGroup({ student, term }: SubjectGroupProps) {
    const [points, setPoints] = useState<SubjectItem[]>();
    const [currentSubject, setCurrentSubject] = useState<StudentSubject>();

    useEffect(() => {
        populateSubjectPointsDataByStudent();
        setCurrentSubject(loadSubjectCookie());
    }, []);

    if (!(currentSubject === undefined)) {
        saveSubjectCookie(currentSubject, 5);
        return (
            <Fragment>
                <a onClick={resetCurrentSubject} className="back-link">← Назад к списку предметов</a>
                <StudentSubjectComponent
                    subjectName={currentSubject.subjectName}
                    student={currentSubject.student}
                    term={term}
                />
            </Fragment>
        );
    }
    return (
        <LoadingPageData isLoading={points === undefined}>
            <h1 className="page-title">Мои предметы</h1>
            <div className="subjects-grid">
                {
                    points?.map((subpoint) => (
                        <div className="subject-card" key={subpoint.subject}>
                            <div className="subject-name">{subpoint.subject}</div>
                            <div className="subject-score">{subpoint.score}</div>
                            <div className="progress-bar">
                                <div className="progress-fill" style={{width: `${subpoint.score}%`}}></div>
                            </div>
                            <div className="progress-text">
                                <span>0</span>
                                <span>100</span>
                            </div>
                            <div className="subject-details">
                                <span>Последнее обновление: {subpoint.lastUpdate}</span>
                                <span>Преподаватель: {subpoint.teacher}</span>
                            </div>
                            <button onClick={() => setCurrentSubject({ subjectName: subpoint.subject, student: student, term: term })} className="btn">Подробнее</button>
                        </div>
                    ))
                }
            </div>
        </LoadingPageData>
    );

    async function populateSubjectPointsDataByStudent() {
        api.get("StudentSubjects/All", {
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
                setPoints(response.data);
            }
        });
    }

    function resetCurrentSubject() {
        setCurrentSubject(undefined);
        removeSubjectCookie(5);
    }
}

export default SubjectsGroup;