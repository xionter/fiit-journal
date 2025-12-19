import { BrowserRouter, Routes, Route } from "react-router-dom"
import LoginPage from "./components/LoginPage"
import Main from "./components/Main"
import "./fiitflow.css"
import { rootLogin, rootMain, rootEdit } from "./components/Navigation"

function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<LoginPage />} />
                <Route path={rootLogin.to} element={<LoginPage />} />
                <Route path={rootMain.to} element={<Main subjectPeaked={false} isEditing={false} />} />
                <Route path={rootEdit.to} element={<Main subjectPeaked={false} isEditing={true} />} />
                <Route path={`${rootMain.to}/:subjectName`} element={<Main subjectPeaked={true} isEditing={false} />} />
            </Routes>
        </BrowserRouter>
    )
}

export default App;