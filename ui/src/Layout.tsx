import { Box, Flex, Text } from "@radix-ui/themes";
import { Link, Outlet } from "@tanstack/react-router";
import React from "react";

const TOP_BAR_ACCENT_BACKGROUND: React.CSSProperties = {
  backgroundColor: "var(--accent-10)",
};

const UNDERLINE_HEADING: React.CSSProperties = {
  textDecoration: "underline",
  textDecorationColor: "var(--accent-2)",
};

export const Layout = () => {
  return (
    <Flex p="0" m="0" style={{ display: 'flex', flexDirection: 'column', width: '100%' }}>
      <Box width="100%" style={TOP_BAR_ACCENT_BACKGROUND} py="4" pl="4">
        <Link title="Go Home" to="/">
          <Text size="8" style={UNDERLINE_HEADING}>
            Reservations @ Mewstel
          </Text>
        </Link>
      </Box>
      <Outlet />
    </Flex>
  );
};
