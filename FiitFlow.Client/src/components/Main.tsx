import { useEffect, useState, Fragment, type ReactElement } from "react"
import { useNavigate, useParams } from "react-router-dom"
import { rootLogin, rootMain, rootEdit } from "./Navigation"
import SubjectsGroup from "./SubjectsGroup"
import type Student from "./Student"
import { loadStudentCookie, loadSubjectCookie, removeSubjectCookie } from "./CookieTools"
import StudentSubjectComponent from "./StudentSubjectComponent"
import SubjectsGroupConfigEditor from "./SubjectsGroupConfigEditor"

interface MainProps {
    subjectPeaked: boolean;
    isEditing: boolean;
}

function Main({ subjectPeaked, isEditing }: MainProps) {
    const navigate = useNavigate();
    const { subjectName } = useParams();
    const [currentStudent, setCurrentStudent] = useState<Student>();
    const [currentTerm, setCurrentTerm] = useState<number>(3);
    const [centralBlock, setCentralBlock] = useState<ReactElement>();

    useEffect(() => {
        window.scrollTo({
            top: 0,
            left: 0,
            behavior: 'smooth'
        });
        setBlockByParams();
    }, [currentTerm, subjectPeaked, isEditing]);

    return (
        <Fragment>
            <header>
                <div className="div-container">
                    <div className="header-content">
                        <div onClick={goToMain} className="logo">
                            <span className="logo-icon">📊</span>
                            FIITFLOW
                        </div>
                        <div className="semester-select">
                            {subjectPeaked || isEditing ? (
                                <nav>
                                    <ul>
                                        <li><a onClick={goToMain}>Главная</a></li>
                                    </ul>
                                </nav>
                            ) : (
                                <select value={currentTerm} onChange={(event) => setCurrentTerm(Number(event.target.value))} className="semester-dropdown">
                                    {
                                        [1, 2, 3, 4].map(num => (
                                            <option key={num} value={num}>Семестр {num}</option>
                                        ))
                                    }
                                </select>
                            )}
                        </div>
                        <div className="user-info">
                            <div className="user-avatar">{currentStudent?.group.substring(0, currentStudent.group.indexOf("-"))}</div>
                            <span>{currentStudent?.lastName} {currentStudent?.firstName}</span>
                            <button onClick={() => { logoutReset(); goToLogin(); } } className="logout-btn">Выход</button>
                        </div>
                    </div>
                </div>
            </header>
            <main className="central-container">
                {centralBlock}
            </main>
            <footer>
                <div className="container">
                    <div className="footer-content">
                        <div className="footer-section">
                            <h3>FIITFLOW</h3>
                            <p>Единая система для отслеживания учебных баллов студентов ФИИТ</p>
                        </div>
                    </div>
                    <div className="copyright">
                        &copy; 2025 FIITFLOW. Все права защищены.
                    </div>
                </div>
            </footer>
        </Fragment>
    );

    function logoutReset() {
        removeSubjectCookie(5);
    }

    function goToLogin() {
        navigate(rootLogin.to, rootLogin.options);
    }

    function goToMain() {
        navigate(rootMain.to, rootMain.options);
    }

    function setBlockByParams() {
        const studentFromCookie = loadStudentCookie();
        if (studentFromCookie === undefined)
            goToLogin();
        else {
            setCurrentStudent(studentFromCookie);
            if (subjectPeaked) {
                const subjectFromCookie = loadSubjectCookie();
                if (subjectFromCookie !== undefined) // && subjectFromCookie.subjectName === subjectName)
                    setCentralBlock(
                        <Fragment>
                            <a onClick={goToMain} className="back-link">← Назад к списку предметов</a>
                            <StudentSubjectComponent
                                subjectName={subjectFromCookie.subjectName}
                                student={studentFromCookie}
                                term={subjectFromCookie.term}
                                score={subjectFromCookie.score}
                            />
                        </Fragment>
                    );
                else
                    goToMain();
            }
            else if (isEditing)
                setCentralBlock(
                    <Fragment>
                        <div className="page-header">
                            <a onClick={goToMain} className="back-link">← Назад к предметам</a>
                            <h1 className="page-title">Редактирование предметов</h1>
                        </div>
                        <SubjectsGroupConfigEditor student={studentFromCookie} term={currentTerm} />
                    </Fragment>
                );
            else
                setCentralBlock(
                    <Fragment>
                        <a onClick={() => navigate(rootEdit.to, rootEdit.options)} className="edit-subject-btn">
                            <span>✏️</span>
                            Изменить или добавить
                        </a>
                        <SubjectsGroup
                            student={studentFromCookie}
                            term={currentTerm}
                        />
                    </Fragment>
                );
        }
    }

    function defaultTerm(studentGroup: string) {
        const month = new Date().getMonth() + 1;
        return Number(studentGroup.at(3)) * 2 - (9 > month && month > 1 ? 0 : 1);
    }
}

export default Main;