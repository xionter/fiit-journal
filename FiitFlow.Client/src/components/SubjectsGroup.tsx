import { useNavigate } from 'react-router-dom'
import { Fragment, useEffect, useState, type ReactElement } from "react"
import { rootMain } from "./Navigation"
import LoadingPageData from "./LoadingPageData"
import type Student from "./Student"
import type PointsItem from "./PointsItem"
import api from "./Api"
import type StudentSubject from "./StudentSubject"
import { loadSubjectCookie, saveSubjectCookie, removeSubjectCookie } from "./CookieTools"
import { string } from 'yup'

interface SubjectGroupProps {
    student: Student;
    term: number;
    isEditing: boolean;
}

function SubjectsGroup({ student, term, isEditing }: SubjectGroupProps) {
    const navigate = useNavigate();
    const [points, setPoints] = useState<PointsItem[]>();
    const [editButton, setEditButton] = useState<ReactElement>();

    useEffect(() => {
        populateSubjectPointsDataByStudent();
    }, [term]);

    useEffect(() => {
        if (isEditing)
            setEditButton(
                <div></div>
            );
        else
            setEditButton(
                <div></div>
            )
    }, [isEditing]);

    return (
        <LoadingPageData isLoading={points === undefined}>
            <h1 className="page-title">Мои предметы</h1>
            <div className="subjects-grid">
                {
                    points?.sort((p1, p2) => p1.subject.toLowerCase().localeCompare(p2.subject.toLowerCase())).map((subpoint) => (
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
                                <span>Последнее обновление: {subpoint.lastUpdate.toLocaleString()}</span>
                                <span>Преподаватель: {subpoint.teacher}</span>
                            </div>
                            <button onClick={() => setCurrentSubject({ subjectName: subpoint.subject, student: student, term: term })} className="btn">Подробнее</button>
                        </div>
                    ))
                }
            </div>
            {editButton}
        </LoadingPageData>
    );

    async function populateSubjectPointsDataByStudent() {
        api.get("StudentSubjects/All", {
            withCredentials: true,
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