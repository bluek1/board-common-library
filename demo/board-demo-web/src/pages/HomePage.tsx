import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

export function HomePage() {
  const { isAuthenticated, user } = useAuth();

  return (
    <div className="space-y-12">
      {/* 히어로 섹션 */}
      <section className="text-center py-12">
        <h1 className="text-4xl font-bold text-gray-900 mb-4">
          게시판 공통 라이브러리 데모
        </h1>
        <p className="text-xl text-gray-600 mb-8 max-w-2xl mx-auto">
          ASP.NET Core 8.0 기반의 재사용 가능한 게시판 API 라이브러리를 체험해보세요.
        </p>
        <div className="flex justify-center space-x-4">
          <Link
            to="/posts"
            className="px-6 py-3 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 transition"
          >
            게시판 보기
          </Link>
          <Link
            to="/questions"
            className="px-6 py-3 bg-white text-blue-600 font-medium rounded-lg border-2 border-blue-600 hover:bg-blue-50 transition"
          >
            Q&A 보기
          </Link>
        </div>
      </section>

      {/* 기능 소개 */}
      <section className="grid md:grid-cols-3 gap-8">
        <div className="bg-white p-6 rounded-xl shadow-md">
          <div className="text-3xl mb-4">📝</div>
          <h3 className="text-lg font-semibold text-gray-800 mb-2">게시물 관리</h3>
          <p className="text-gray-600 text-sm">
            게시물 CRUD, 조회수, 상단고정, 임시저장 등 다양한 게시판 기능을 제공합니다.
          </p>
        </div>
        <div className="bg-white p-6 rounded-xl shadow-md">
          <div className="text-3xl mb-4">💬</div>
          <h3 className="text-lg font-semibold text-gray-800 mb-2">댓글 & 좋아요</h3>
          <p className="text-gray-600 text-sm">
            댓글, 대댓글, 좋아요, 북마크 기능으로 사용자 참여를 높입니다.
          </p>
        </div>
        <div className="bg-white p-6 rounded-xl shadow-md">
          <div className="text-3xl mb-4">❓</div>
          <h3 className="text-lg font-semibold text-gray-800 mb-2">Q&A 게시판</h3>
          <p className="text-gray-600 text-sm">
            질문과 답변, 답변 채택, 추천 시스템을 지원하는 Q&A 게시판입니다.
          </p>
        </div>
        <div className="bg-white p-6 rounded-xl shadow-md">
          <div className="text-3xl mb-4">🔒</div>
          <h3 className="text-lg font-semibold text-gray-800 mb-2">인증 & 권한</h3>
          <p className="text-gray-600 text-sm">
            JWT 인증, 역할 기반 권한 관리로 안전한 서비스를 구축합니다.
          </p>
        </div>
        <div className="bg-white p-6 rounded-xl shadow-md">
          <div className="text-3xl mb-4">📎</div>
          <h3 className="text-lg font-semibold text-gray-800 mb-2">파일 첨부</h3>
          <p className="text-gray-600 text-sm">
            파일 업로드, 썸네일 생성, 파일 검증 기능을 포함합니다.
          </p>
        </div>
        <div className="bg-white p-6 rounded-xl shadow-md">
          <div className="text-3xl mb-4">🔍</div>
          <h3 className="text-lg font-semibold text-gray-800 mb-2">검색 & 필터</h3>
          <p className="text-gray-600 text-sm">
            키워드 검색, 태그 검색, 다양한 필터링 옵션을 지원합니다.
          </p>
        </div>
      </section>

      {/* 빠른 시작 */}
      {isAuthenticated ? (
        <section className="bg-white p-8 rounded-xl shadow-md">
          <h2 className="text-2xl font-bold text-gray-800 mb-4">
            안녕하세요, {user?.displayName || user?.username}님! 👋
          </h2>
          <p className="text-gray-600 mb-6">
            지금 바로 게시물을 작성하거나 Q&A에 질문을 올려보세요.
          </p>
          <div className="flex space-x-4">
            <Link
              to="/posts/new"
              className="px-4 py-2 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 transition"
            >
              새 게시물 작성
            </Link>
            <Link
              to="/questions/new"
              className="px-4 py-2 bg-green-600 text-white font-medium rounded-lg hover:bg-green-700 transition"
            >
              질문하기
            </Link>
          </div>
        </section>
      ) : (
        <section className="bg-blue-50 p-8 rounded-xl border border-blue-100">
          <h2 className="text-2xl font-bold text-gray-800 mb-4">시작하기</h2>
          <p className="text-gray-600 mb-6">
            로그인하여 게시물 작성, 댓글, 좋아요 등의 기능을 사용해보세요.
          </p>
          <div className="flex space-x-4">
            <Link
              to="/login"
              className="px-4 py-2 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 transition"
            >
              로그인
            </Link>
            <Link
              to="/register"
              className="px-4 py-2 bg-white text-blue-600 font-medium rounded-lg border border-blue-200 hover:bg-blue-50 transition"
            >
              회원가입
            </Link>
          </div>
        </section>
      )}
    </div>
  );
}
