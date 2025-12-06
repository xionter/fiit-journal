import { Fragment, useEffect, useState } from "react"
import "./SubjectsGroup.css"

interface SubjectPoints {
    subject: string;
    teacher: string;
    score: number;
    lastUpdate: Date;
}

interface Student {
    studentName: string;
    group: string;
}

function SubjectsGroup({ studentName, group }: Student) {
    const [points, setPoints] = useState<SubjectPoints[]>();

    useEffect(() => {
        populateSubjectPointsData();
    }, []);

    //if (points === undefined)
    //    populateSubjectPointsData();

    return (
        <Fragment>
            <h1 className="page-title">Мои предметы {studentName}</h1>
            <div className="subjects-grid">
                {
                    points?.map((subpoint) => (
                        <div className="subject-card" key={subpoint.subject}>
                            <div className="subject-name">{subpoint.subject}</div>
                            <div className="subject-score">{subpoint.score}</div>
                            <div className="progress-bar">
                                <div className="progress-fill"></div>
                            </div>
                            <div className="progress-text">
                                <span>0</span>
                                <span>100</span>
                            </div>
                            <div className="subject-details">
                                <span>Последнее обновление: {subpoint.lastUpdate.toDateString()}</span>
                                <span>Преподаватель: {subpoint.teacher}</span>
                            </div>
                            <a href={subpoint.subject} className="btn">Подробнее</a>
                        </div>
                    ))
                }
            </div>
        </Fragment>
    );

    async function populateSubjectPointsData() {
        const response = await fetch(window.location.protocol + "//" + window.location.host + "/api/studentsubjects");
        if (response.ok) {
            const data = await response.json();
            setPoints(data);
        }
    }
}

export default SubjectsGroup;