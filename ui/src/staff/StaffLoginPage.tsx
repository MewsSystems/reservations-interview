import { useState } from "react";
import { Box, Button, Card, Flex, Heading, Text, TextField } from "@radix-ui/themes";
import { useNavigate } from "@tanstack/react-router";
import { staffLogin } from "./api";

export function StaffLoginPage() {
  const [accessCode, setAccessCode] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setLoading(true);

    const success = await staffLogin(accessCode);

    setLoading(false);

    if (success) {
      navigate({ to: "/staff/reservations" });
    } else {
      setError("Invalid access code. Please try again.");
    }
  }

  return (
    <Flex align="center" justify="center" pt="9">
      <Card size="4" style={{ width: 360 }}>
        <Heading size="6" mb="4" align="center">
          Staff Login
        </Heading>
        <form onSubmit={handleSubmit}>
          <Flex direction="column" gap="4">
            <Box>
              <Text as="label" size="2" weight="medium" htmlFor="access-code">
                Access Code
              </Text>
              <TextField.Root
                id="access-code"
                type="password"
                placeholder="Enter staff access code"
                value={accessCode}
                onChange={(e) => setAccessCode(e.target.value)}
                mt="1"
              />
            </Box>
            {error && (
              <Text color="red" size="2">
                {error}
              </Text>
            )}
            <Button type="submit" disabled={loading || !accessCode}>
              {loading ? "Checking..." : "Login"}
            </Button>
          </Flex>
        </form>
      </Card>
    </Flex>
  );
}
