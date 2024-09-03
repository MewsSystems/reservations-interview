export const styles: { [key: string]: React.CSSProperties } = {
    loginContainer: {
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
    },
    card: {
        borderRadius: '8px',
        boxShadow: '0 4px 8px rgba(0, 0, 0, 0.1)',
        padding: '2rem',
        maxWidth: '400px',
        width: '100%',
    },
    loginForm: {
        width: '100%',
        display: 'flex',
        flexDirection: 'column' as 'column',
    },
    formGroup: {
        marginBottom: '1rem',
    },
    formLabel: {
        display: 'block',
        marginBottom: '0.5rem',
        fontWeight: 'bold',
    },
    formInput: {
        width: '100%',
        padding: '0.5rem',
        border: '1px solid #ddd',
        borderRadius: '4px',
        boxSizing: 'border-box',
    },
    formButton: {
        width: '100%',
        padding: '0.75rem',
        backgroundColor: "var(--accent-10)",
        color: 'white',
        border: 'none',
        borderRadius: '4px',
        cursor: 'pointer',
        fontSize: '1rem',
        marginTop: '1rem',
    },
    formButtonHover: {
        backgroundColor: "var(--accent-10)",
    },
};