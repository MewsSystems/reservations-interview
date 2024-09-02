
export const IsAuthenticated = () => {
    return document.cookie.split(';').some((cookie) => cookie.startsWith('access='));
}