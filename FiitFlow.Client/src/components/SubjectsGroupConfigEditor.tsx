import { useNavigate } from 'react-router-dom'
import { Fragment, useEffect, useState } from "react"
import * as yup from "yup"
import type Student from "./Student"
import api from "./Api"
import { saveStudentCookie, loadStudentCookie } from "./CookieTools"
import { rootMain } from "./Navigation"
import { useForm, useFieldArray } from "react-hook-form";
import { yupResolver } from "@hookform/resolvers/yup";
import type PointsItem from './PointsItem'

const googleSheetRegex = /^https?:\/\/docs\.google\.com\/spreadsheets\/(?:u\/\d+\/)?d\/([a-zA-Z0-9-_]+)(?:\/[^\s]*)?$/;

interface ConfigEditorProps {
    student: Student;
}

interface SubjectConfigInput {
    name: string;
    link: string;
    group: string;
}

interface FormInputs {
    subjects: SubjectConfigInput[];
}

const subjectSchema = yup.object({
    name: yup.string().required("Название предмета").matches(/^[А-ЯЁа-яё -\.]+$/, "Неверный формат"),
    link: yup.string().required("Ссылка на таблицу").matches(googleSheetRegex, "Неверный формат"),
    group: yup.string().required("Введите группу").matches(/^ФТ-\d\d\d-\d$/, "Неверный формат")
});

const schema = yup.object({
    subjects: yup.array().of(subjectSchema).min(1, "Добавьте хотя бы одного студента")
}).required();

export default function SubjectsGroupConfigEditor({ student }: ConfigEditorProps) {
    const navigate = useNavigate();

    const {
        control,
        register,
        handleSubmit,
        setValue,
        setError,
        formState: { errors, isValid, isSubmitting }
    } = useForm<FormInputs>({
        resolver: yupResolver(schema),
        mode: "onChange",
        defaultValues: {
            subjects: [{ name: "", link: "", group: "" }]
        }
    });

    const { fields, append, remove } = useFieldArray({
        control,
        name: "subjects"
    });

    useEffect(() => {
        const configs: SubjectConfigInput[] = loadStudentConfigSubjects();
        for (const [i, subjectConfig] of configs.entries()) {
            setValue(`subjects.${i}.name`, subjectConfig.name);
            setValue(`subjects.${i}.link`, subjectConfig.link);
            setValue(`subjects.${i}.group`, subjectConfig.group);
        }
    }, []);

    return (
        <div className="login-container">
            <div className="login-box">
                <div className="login-logo">
                    <h1>
                        <span className="logo-icon">📊</span>
                        FIITFLOW
                    </h1>
                    <p>Вход в систему</p>
                </div>

                <form onSubmit={handleSubmit(onSubmit)} className="login-form">
                    {fields.map((field, index) => (
                        <div key={field.id} className="subject-card">
                            <div className="subject-name">{subpoint.subject}</div>

                            <div className="form-group">
                                <label>Фамилия</label>
                                <input
                                    {...register(`students.${index}.lastName`)}
                                    className={`input ${errors.students?.[index]?.lastName ? 'input-error' : ''}`}
                                    placeholder="Введите вашу фамилию"
                                />
                                {errors.students?.[index]?.lastName && (
                                    <p className="text-red-500 text-sm">
                                        {errors.students[index]!.lastName!.message}
                                    </p>
                                )}
                            </div>

                            <div className="form-group">
                                <label>Имя</label>
                                <input
                                    {...register(`students.${index}.firstName`)}
                                    className={`input ${errors.students?.[index]?.firstName ? 'input-error' : ''}`}
                                    placeholder="Введите ваше имя"
                                />
                                {errors.students?.[index]?.firstName && (
                                    <p className="text-red-500 text-sm">
                                        {errors.students[index]!.firstName!.message}
                                    </p>
                                )}
                            </div>

                            <div className="form-group">
                                <label>Группа</label>
                                <input
                                    {...register(`students.${index}.group`)}
                                    className={`input ${errors.students?.[index]?.group ? 'input-error' : ''}`}
                                    placeholder="Например: ФТ-201-1"
                                />
                                {errors.students?.[index]?.group && (
                                    <p className="text-red-500 text-sm">
                                        {errors.students[index]!.group!.message}
                                    </p>
                                )}
                            </div>

                            {fields.length > 1 && (
                                <button
                                    type="button"
                                    className="login-btn secondary"
                                    onClick={() => remove(index)}
                                >
                                    Удалить этого студента
                                </button>
                            )}
                        </div>
                    ))}

                    <button
                        type="button"
                        className="add-subject-btn"
                        onClick={() => append({ firstName: "", lastName: "", group: "" })}
                    > + </button>

                    <button
                        type="submit"
                        className="login-btn"
                        disabled={!isValid || isSubmitting}
                    >
                        {isSubmitting ? "Отправка..." : "Сохранить"}
                    </button>

                    {errors.root && (
                        <p className="text-red-500 text-sm">{errors.root.message}</p>
                    )}
                </form>

                <div className="login-info">
                    <p>Для входа используйте реальные данные</p>
                    <p>Система предназначена для студентов ФИИТ УрФУ</p>
                </div>
            </div>
        </div>
    );

    function onSubmit() {

    }

    function loadStudentConfigSubjects() {
        return []
    }
}
