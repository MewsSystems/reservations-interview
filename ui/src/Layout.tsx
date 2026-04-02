import { Box, Button, Flex, Text } from "@radix-ui/themes";
import { Link, Outlet, useRouter } from "@tanstack/react-router";
import React from "react";
import { useAuth } from "./staff/AuthContext";

const TOP_BAR_ACCENT_BACKGROUND: React.CSSProperties = {
  backgroundColor: "var(--accent-10)",
};

const UNDERLINE_HEADING: React.CSSProperties = {
  textDecoration: "underline",
  textDecorationColor: "var(--accent-2)",
};

export const Layout = () => {
  const router = useRouter();
  const { isAuthed, logout } = useAuth();

  async function handleLogout() {
    await logout();
    router.navigate({ to: "/" });
  }

  return (
    <Box p="0" m="0">
      <Flex width="100%" style={TOP_BAR_ACCENT_BACKGROUND} py="4" px="4" align="center" justify="between">
        <Link title="Go Home" to="/">
          <Text size="8" style={UNDERLINE_HEADING}>
            Reservations @ Mewstel
          </Text>
        </Link>
        {isAuthed && (
          <Button variant="soft" color="red" size="2" onClick={handleLogout}>
            Logout
          </Button>
        )}
      </Flex>
      <Outlet />
    </Box>
  );
};
