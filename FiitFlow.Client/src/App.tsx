import { useEffect, useState, Fragment } from 'react';
import SubjectsGroup from "./components/SubjectsGroup"
import './App.css';
import LoginPage from './components/LoginPage';

interface Forecast {
    date: string;
    temperatureC: number;
    temperatureF: number;
    summary: string;
}

function App() {
    const [forecasts, setForecasts] = useState<Forecast[]>();

    useEffect(() => {
        populateWeatherData();
    }, []);

    let currentStudent: string = "Пеганов Артём";
    let centralBlock;

    if (currentStudent === undefined)
        centralBlock = <LoginPage />
    else
        centralBlock = <SubjectsGroup studentName={currentStudent} group="201" />

    return (
        <>
            <header>
                <div className="container">
                    <div className="header-content">
                        <div className="logo">
                            <span className="logo-icon">📊</span>
                            FIITFLOW
                        </div>
                        <nav>
                            <ul>
                                <li><a href="index.html">Главная</a></li>
                                <li><a href="subjects.html">Предметы</a></li>
                                <li><a href="analytics.html">Аналитика</a></li>
                                <li><a href="settings.html">Настройки</a></li>
                            </ul>
                        </nav>
                        <div className="user-info">
                            <div className="user-avatar">;)</div>
                            <span>{currentStudent}</span>
                        </div>
                    </div>
                </div>
            </header>
            <div className="container">
                {centralBlock}
            </div>
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
        </>
    );

    async function populateWeatherData() {
        const response = await fetch('weatherforecast');
        if (response.ok) {
            const data = await response.json();
            setForecasts(data);
        }
    }
}

export default App;