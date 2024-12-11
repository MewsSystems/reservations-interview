import { useState } from 'react';
import ky from "ky";
import { useNavigate } from '@tanstack/react-router';

export function LoginPage() {
    const [username, setUsername] = useState<string>('');
    const [password, setPassword] = useState<string>('');
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    const handleSubmit = async (event: React.FormEvent) => {
        event.preventDefault();

        if (!username || !password) {
            setError('Username and password are required.');
            return;
        }

        setError(null);

        try {
            await ky.get("api/staff/login", {
                headers: {
                    "X-Staff-Code": password
                }
            });
            await ky.get("api/staff/check");
        } catch (error) {
            console.log("Login error:", error);
            setError("Invalid credentials");
            return;
        }

        navigate({to: "/staff"});
    };

    return (
        <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
            <form onSubmit={handleSubmit} style={{ width: '300px', textAlign: 'center' }}>
                <h2>Login</h2>

                {error && <p style={{ color: 'red' }}>{error}</p>}

                <div style={{ marginBottom: '10px' }}>
                    <input
                        type="text"
                        placeholder="Username"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        style={{ padding: '10px', width: '100%' }}
                    />
                </div>

                <div style={{ marginBottom: '10px' }}>
                    <input
                        type="password"
                        placeholder="Password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        style={{ padding: '10px', width: '100%' }}
                    />
                </div>

                <button type="submit" style={{ padding: '10px', width: '100%', background: '#007BFF', color: 'white', border: 'none' }}>
                    Login
                </button>
            </form>
        </div>
    );
}
