import { useNavigate } from 'react-router-dom'
import { Fragment, useEffect, useState } from "react"
import * as yup from "yup"
import type Student from "./Student"
import api from "./Api"
import { saveStudentCookie, loadStudentCookie } from "./CookieTools"
import { rootMain, rootEdit } from "./Navigation"
import { useForm, useFieldArray } from "react-hook-form";
import { yupResolver } from "@hookform/resolvers/yup";
import type PointsItem from './PointsItem'
import SubjectsGroup from './SubjectsGroup'
import LoadingPageData from "./LoadingPageData"

const googleSheetRegex = /^https?:\/\/docs\.google\.com\/spreadsheets\/(?:u\/\d+\/)?d\/([a-zA-Z0-9-_]+)(?:\/[^\s]*)?$/;

interface ConfigEditorProps {
    student: Student;
    term: number;
}

interface SubjectConfigInput {
    baseName: string;
    name: string;
    link: string;
}

interface FormSubjects {
    subjects: SubjectConfigInput[];
}

const subjectSchema = yup.object({
    baseName: yup.string().required(),
    name: yup.string().required("Название предмета").matches(/^[А-ЯЁа-яё -\.]+$/, "Неверный формат"),
    link: yup.string().required("Ссылка на таблицу").matches(googleSheetRegex, "Неверный формат")
});

const schema = yup.object({
    subjects: yup.array().of(subjectSchema).min(1, "Данных вашей группы ещё нет в базе, ткните куру").required()
}).required();

export default function SubjectsGroupConfigEditor({ student, term }: ConfigEditorProps) {
    const navigate = useNavigate();
    const [baseSubCon, setBaseSubCon] = useState<SubjectConfigInput[]>();

    useEffect(() => {
        loadStudentConfigSubjects();
    }, []);

    const {
        control,
        register,
        handleSubmit,
        setError,
        formState: { errors, isValid, isSubmitting }
    } = useForm<FormSubjects>({
        resolver: yupResolver(schema),
        mode: "onChange",
        defaultValues: {
            subjects: baseSubCon
        }
    });

    const { fields, append, remove } = useFieldArray({
        control,
        name: "subjects"
    });

    return (
        <LoadingPageData isLoading={baseSubCon === undefined}>
            
            <form onSubmit={handleSubmit(data => onSubmit(data))} className="login-form">
                {fields.map((field, index) => (
                    <div key={field.id} className="subject-card">
                        <div className="subject-name">{field.baseName}</div>

                        <div className="form-group">
                            <label>Предмет</label>
                            <input
                                {...register(`subjects.${index}.name`)}
                                className={`input ${errors.subjects?.[index]?.name ? 'input-error' : ''}`}
                                placeholder="Введите название предмета"
                            />
                            {errors.subjects?.[index]?.name && (
                                <p className="text-red-500 text-sm">
                                    {errors.subjects[index]!.name!.message}
                                </p>
                            )}
                        </div>

                        <div className="form-group">
                            <label>Ссылка на таблицу</label>
                            <input
                                {...register(`subjects.${index}.link`)}
                                className={`input ${errors.subjects?.[index]?.link ? 'input-error' : ''}`}
                                placeholder="Вставьте ссылку на таблицу"
                            />
                            {errors.subjects?.[index]?.link && (
                                <p className="text-red-500 text-sm">
                                    {errors.subjects[index]!.link!.message}
                                </p>
                            )}
                        </div>

                        {fields.length > 1 && (
                            <button
                                type="button"
                                className="login-btn secondary"
                                onClick={() => remove(index)}
                            >
                                Удалить
                            </button>
                        )}
                    </div>
                ))}

                <button
                    type="button"
                    className="add-subject-btn"
                    onClick={() => append({ baseName: "", name: "", link: "" })}
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
        </LoadingPageData>
    );

    async function onSubmit(data: FormSubjects) {
        const isOk = await api.post(`ConfigEdit/SetConfigs`, data, {
            withCredentials: true,
            params: {
                firstName: student.firstName,
                lastName: student.lastName,
                group: student.group,
                time: Date.now()
            }
        }).then(response => {
            if (response.status == 200)
                return response.data;
            return null;
        });
        if (isOk)
            navigate(rootMain.to, rootMain.options);
        else
            setError("root.serverError", {
                type: "server",
                message: "Ошибка"
            });
    }

    function loadStudentConfigSubjects() {
        return api.get<SubjectConfigInput[]>("ConfigEdit/GetConfigs", {
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
                setBaseSubCon(response.data);
            }
        });
    }
}
