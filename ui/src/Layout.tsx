import { Box, Button, Flex, Text } from "@radix-ui/themes";
import { Link, Outlet, useNavigate } from "@tanstack/react-router";
import React from "react";
import { useAuth } from "./utils/auth";

const TOP_BAR_ACCENT_BACKGROUND: React.CSSProperties = {
  backgroundColor: "var(--accent-10)",
};

const HEADING: React.CSSProperties = {
  textDecoration: "none",
};

export const Layout = () => {
  const { isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate({ to: "/" });
  }

  return (
    <Box p="0" m="0">
      <Flex width="100%" style={TOP_BAR_ACCENT_BACKGROUND} py="4" px="4" justify="between" align="center">
        <Link title="Go Home" to="/" style={HEADING}>
          <Text size="8" weight="bold">
            Mewstel
          </Text>
        </Link>
        {isAuthenticated && (
          <Flex align="center" gap="3">
            <Link to="/staff/reservations" style={HEADING}>
              <Text size="3" weight="medium">Reservations</Text>
            </Link>
            <Link to="/staff/rooms" style={HEADING}>
              <Text size="3" weight="medium">Rooms</Text>
            </Link>
            <Button size="1" color="gray" variant="outline" onClick={handleLogout}>
              Logout
            </Button>
          </Flex>
        )}
      </Flex>
      <Outlet />
    </Box>
  );
};
