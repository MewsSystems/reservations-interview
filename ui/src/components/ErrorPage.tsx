import { Flex, Heading, Text, Button } from "@radix-ui/themes";
import { useRouter, Link } from "@tanstack/react-router";

interface ErrorPageProps {
  statusCode?: number;
  message?: string;
}

const CONFIG: Record<
  number,
  { title: string; description: string; color: "red" | "orange" }
> = {
  400: {
    title: "Bad Request",
    description: "The request could not be understood by the server.",
    color: "orange",
  },
  401: {
    title: "Unauthorised",
    description: "You need to be logged in to access this page.",
    color: "orange",
  },
  403: {
    title: "Forbidden",
    description: "You don't have permission to access this resource.",
    color: "orange",
  },
  404: {
    title: "Page Not Found",
    description: "The page you're looking for doesn't exist or has been moved.",
    color: "orange",
  },
  500: {
    title: "Server Error",
    description: "Something went wrong on our end. Please try again later.",
    color: "red",
  },
  503: {
    title: "Service Unavailable",
    description: "The service is temporarily unavailable. Please try again later.",
    color: "red",
  },
};

function getConfig(code: number) {
  if (CONFIG[code]) return { ...CONFIG[code], code };
  if (code >= 500) return { ...CONFIG[500], code };
  if (code >= 400) return { ...CONFIG[400], code };
  return { title: "Unexpected Error", description: "An unexpected error occurred.", color: "red" as const, code };
}

export function ErrorPage({ statusCode = 500, message }: ErrorPageProps) {
  const router = useRouter();
  const { title, description, color, code } = getConfig(statusCode);

  return (
    <Flex
      direction="column"
      align="center"
      justify="center"
      gap="4"
      style={{ minHeight: "60vh" }}
      px="4"
    >
      <Heading size="9" color={color} style={{ fontSize: "6rem", lineHeight: 1 }}>
        {code}
      </Heading>
      <Heading size="6" as="h2">
        {title}
      </Heading>
      <Text size="3" color="gray" align="center" style={{ maxWidth: 480 }}>
        {message ?? description}
      </Text>
      <Flex gap="3" mt="4">
        <Button variant="soft" onClick={() => router.history.back()}>
          Go Back
        </Button>
        <Button onClick={() => router.navigate({ to: "/" })}>
          Go Home
        </Button>
        <Button variant="outline" asChild>
          <Link to="/staff/login">Staff Login</Link>
        </Button>
      </Flex>
    </Flex>
  );
}
