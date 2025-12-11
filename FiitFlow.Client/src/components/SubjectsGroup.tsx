import { useNavigate } from 'react-router-dom'
import { Fragment, useEffect, useState } from "react"
import { rootMain } from "./Navigation"
import LoadingPageData from "./LoadingPageData"
import type Student from "./Student"
import type SubjectItem from "./SubjectItem"
import api from "./Api"
import type StudentSubject from "./StudentSubject"
import { loadSubjectCookie, saveSubjectCookie, removeSubjectCookie } from "./CookieTools"

interface SubjectGroupProps {
    student: Student;
    term: number;
}

function SubjectsGroup({ student, term }: SubjectGroupProps) {
    const navigate = useNavigate();
    const [points, setPoints] = useState<SubjectItem[]>();

    useEffect(() => {
        populateSubjectPointsDataByStudent();
    }, [term]);

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

    function setCurrentSubject(subject: StudentSubject) {
        saveSubjectCookie(subject, 5);
        navigate(`${rootMain.to}/${subject.subjectName}`, rootMain.options);
    }
}

export default SubjectsGroup;