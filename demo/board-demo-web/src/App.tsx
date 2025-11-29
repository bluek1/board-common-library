import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import { Layout } from './components/Layout';
import { ProtectedRoute } from './components/ProtectedRoute';
import {
  LoginPage,
  RegisterPage,
  HomePage,
  PostListPage,
  PostDetailPage,
  PostCreatePage,
  QuestionListPage,
  QuestionDetailPage,
  QuestionCreatePage,
} from './pages';

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          {/* Public routes */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          
          {/* Protected routes */}
          <Route element={<ProtectedRoute><Layout /></ProtectedRoute>}>
            <Route path="/" element={<HomePage />} />
            <Route path="/posts" element={<PostListPage />} />
            <Route path="/posts/create" element={<PostCreatePage />} />
            <Route path="/posts/:id" element={<PostDetailPage />} />
            <Route path="/questions" element={<QuestionListPage />} />
            <Route path="/questions/create" element={<QuestionCreatePage />} />
            <Route path="/questions/:id" element={<QuestionDetailPage />} />
          </Route>
          
          {/* Fallback */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;
