import * as Form from '@radix-ui/react-form';
import { Section } from '@radix-ui/themes';
import { useState } from 'react';
import { useShowInfoToast } from '../../utils/toasts';
import { login } from '../api';
import { useNavigate } from '@tanstack/react-router';
import { styles } from './LoginPageStyles';

export const LoginPage = () => {
    const [accessCode, setAccessCode] = useState('');
    const navigate = useNavigate();
    const showLoginFailedToast = useShowInfoToast("Login failed.");

    function handleSubmit(evt: React.FormEvent<HTMLFormElement>) {
        evt.preventDefault();
        login(accessCode, () => {
            navigate({ to: '/staff/reservations', replace: true });
        }, () => {
            showLoginFailedToast();
        })
    };

    return (
        <div style={styles.loginContainer}>
            <Section size="2" px="2">
                <div style={styles.card}>
                    <Form.Root onSubmit={(e) => handleSubmit(e)} style={styles.loginForm}>
                        <Form.Field name="accessCode">
                            <div style={styles.formGroup}>
                                <Form.Label style={styles.formLabel}>Access Code</Form.Label>
                                <Form.Control asChild>
                                    <input
                                        type="password"
                                        value={accessCode}
                                        onChange={(e) => setAccessCode(e.target.value)}
                                        style={styles.formInput}
                                        required
                                    />
                                </Form.Control>
                            </div>
                        </Form.Field>
                        <Form.Submit asChild>
                            <button style={styles.formButton} type="submit">Login</button>
                        </Form.Submit>
                    </Form.Root>
                </div>
            </Section>
        </div>
    );
};
