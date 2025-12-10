import { useEffect, useState, Fragment, type ReactElement } from 'react'
import { rootLogin } from "./Navigation"
import SubjectsGroup from "./SubjectsGroup"
import type Student from "./Student"
import { loadStudentCookie, removeStudentCookie, removeSubjectCookie, saveStudentCookie } from './CookieTools'
import { useNavigate } from 'react-router-dom'

function Main() {
    const navigate = useNavigate();
    const [currentStudent, setCurrentStudent] = useState<Student>();
    const [currentTerm, setCurrentTerm] = useState<number>(1);
    const [centralBlock, setCentralBlock] = useState<ReactElement>();

    useEffect(() => {
        const studentFromCookie = loadStudentCookie();
        if (studentFromCookie === undefined)
            goToLogin();
        else {
            setCurrentStudent(studentFromCookie);
            setCentralBlock(
                <SubjectsGroup
                    student={studentFromCookie}
                    term={currentTerm}
                />
            );
        }
    }, [currentTerm]);

    return (
        <Fragment>
            <header>
                <div className="div-container">
                    <div className="header-content">
                        <div className="logo">
                            <span className="logo-icon">📊</span>
                            FIITFLOW
                        </div>
                        <div className="semester-select">
                            <select onChange={(event) => setCurrentTerm(Number(event.target.value))} className="semester-dropdown">
                                {
                                    [1, 2, 3, 4, 5, 6, 7, 8].map(num => (
                                        <option key={num} value={num}>Семестр {num}</option>
                                    ))
                                }
                            </select>
                        </div>
                        <div className="user-info">
                            <div className="user-avatar">ФИИТ</div>
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
                        <div className="footer-section">
                            <h3>Навигация</h3>
                            <ul>
                                <li><a href="index.html">Главная</a></li>
                                <li><a href="subjects.html">Предметы</a></li>
                                <li><a href="analytics.html">Аналитика</a></li>
                                <li><a href="settings.html">Настройки</a></li>
                            </ul>
                        </div>
                        <div className="footer-section">
                            <h3>Контакты</h3>
                            <ul>
                                <li>Email: support@fiitflow.ru</li>
                                <li>Телеграм: @fiitflow_support</li>
                                <li>Кампус: УрФУ, корпус ФИИТ</li>
                            </ul>
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
}

export default Main;