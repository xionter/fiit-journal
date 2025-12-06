import { Fragment, useEffect, useState } from "react"
import type Student from "./Student";

interface LoginProps {
    setCurrentStudent: Function;
}

function LoginPage({ setCurrentStudent }: LoginProps) {
    return (
        <p>Hello Student!<button onClick={() => setCurrentStudent({ firstName: "Пеганов", secondName: "Артем", group: "ФТ-201-2" })}>Set Name</button></p>
    );
}

export default LoginPage;