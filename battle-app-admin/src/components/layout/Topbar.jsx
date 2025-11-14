import { Flex, Heading } from '@chakra-ui/react';
export default function Topbar() {
  return (
    <Flex h="14" align="center" px="6" borderBottom="1px solid" borderColor="gray.200" bg="white">
      <Heading size="md">Battle Admin</Heading>
    </Flex>
  );
}
