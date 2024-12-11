import { useState } from "react";

export function useEmailValidation(initialValue: string) {
  const [email, setEmail] = useState(initialValue);
  const [error, setError] = useState<string | null>(null);

  const handleEmailChange = (evt: React.ChangeEvent<HTMLInputElement>) => {
    const emailValue = evt.target.value;
    setEmail(emailValue);

    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    if (!emailRegex.test(emailValue)) {
      setError("Please enter a valid email address.");
    } else {
      setError(null);
    }
  };

  return { email, error, handleEmailChange };
}
